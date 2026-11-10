using System;
using System.Collections.Generic;

namespace MusicPlaylistApp
{
    
    public interface IPlayable
    {
        string Title { get; }
        int DurationSeconds { get; }
    }

    
    public class Song : IPlayable
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public int DurationSeconds { get; set; }

        public Song(string title, string artist, int durationSeconds)
        {
            Title = title;
            Artist = artist;
            DurationSeconds = durationSeconds;
        }

        public override string ToString()
        {
            TimeSpan time = TimeSpan.FromSeconds(DurationSeconds);
            return $"{Artist} - {Title} [{time:mm\\:ss}]";
        }
    }

    
    public class TrackStartedEventArgs<T> : EventArgs where T : IPlayable
    {
        public T Track { get; }
        public int Position { get; }

        public TrackStartedEventArgs(T track, int position)
        {
            Track = track;
            Position = position;
        }
    }


    public class Playlist<T> where T : IPlayable
    {
        private readonly List<T> _tracks = new();
        private int _currentIndex = 0;

       
        private Func<int, int, int>? _nextTrackRule = null;

       
        public event EventHandler<TrackStartedEventArgs<T>>? TrackStarted;

        
        protected virtual void OnTrackStarted(T track, int position)
        {
            TrackStarted?.Invoke(this, new TrackStartedEventArgs<T>(track, position));
        }

     
        public void Add(T track)
        {
            _tracks.Add(track);
        }

        public void Next()
        {
            if (_tracks.Count == 0)
            {
                Console.WriteLine("Плейлист порожній!");
                return;
            }

           
            if (_nextTrackRule != null)
            {
                _currentIndex = _nextTrackRule(_currentIndex, _tracks.Count);
                
                _currentIndex = Math.Clamp(_currentIndex, 0, _tracks.Count - 1);
            }
            else
            {
                _currentIndex = (_currentIndex + 1) % _tracks.Count;
            }

            OnTrackStarted(_tracks[_currentIndex], _currentIndex);
        }

        public int GetTotalDuration()
        {
            int total = 0;
            ForEach(track => total += track.DurationSeconds);
            return total;
        }

        
        public void ForEach(Action<T> action)
        {
            foreach (var track in _tracks)
            {
                action(track);
            }
        }

        
        public void SetNextTrackRule(Func<int, int, int>? rule)
        {
            _nextTrackRule = rule;
        }

        
        public int Count => _tracks.Count;
        public int CurrentIndex => _currentIndex;

        public void DisplayPlaylist()
        {
            if (_tracks.Count == 0)
            {
                Console.WriteLine("Плейлист порожній.");
                return;
            }

            Console.WriteLine("\n МІЙ ПЛЕЙЛИСТ (LANA DEL REY) ");
            for (int i = 0; i < _tracks.Count; i++)
            {
                string marker = (i == _currentIndex) ? " ▶ [ПОТОЧНИЙ]" : "   ";
                Console.WriteLine($"{i + 1}.{marker} {_tracks[i]}");
            }

            TimeSpan totalTime = TimeSpan.FromSeconds(GetTotalDuration());
            Console.WriteLine($"Загальна тривалість: {totalTime:mm\\:ss} ({GetTotalDuration()} сек)\n");
        }
    }

    
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            Playlist<Song> playlist = new();

           
            playlist.Add(new Song("Video Games", "Lana Del Rey", 282));
            playlist.Add(new Song("Summertime Sadness", "Lana Del Rey", 265));
            playlist.Add(new Song("Born to Die", "Lana Del Rey", 286));
            playlist.Add(new Song("Young and Beautiful", "Lana Del Rey", 236));
            playlist.Add(new Song("West Coast", "Lana Del Rey", 257));

            
            List<string> listeningHistory = new();

          
            playlist.TrackStarted += DisplayCurrentTrack;

            
            playlist.TrackStarted += (sender, e) =>
            {
                string logEntry = $"[Історія] Прослухано трек #{e.Position + 1}: {e.Track}";
                listeningHistory.Add(logEntry);
            };

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n");
                Console.WriteLine(" МЕНЮ МУЗИЧНОГО ПЛЕЄРА ");
                Console.WriteLine("");
                Console.WriteLine("1. Вихід");
                Console.WriteLine("2. Додати трек");
                Console.WriteLine("3. Показати плейлист");
                Console.WriteLine("4. Наступний трек");
                Console.WriteLine("5. Обрати режим відтворення");
                Console.WriteLine("6. Показати історію прослуховування");
                Console.Write("\nОберіть пункт меню: ");

                string? input = Console.ReadLine();
                Console.WriteLine();

                switch (input)
                {
                    case "1":
                        running = false;
                        Console.WriteLine("Роботу програми завершено.");
                        break;

                    case "2":
                        AddSongMenu(playlist);
                        break;

                    case "3":
                        playlist.DisplayPlaylist();
                        break;

                    case "4":
                        playlist.Next();
                        break;

                    case "5":
                        SelectPlaybackModeMenu(playlist);
                        break;

                    case "6":
                        ShowHistoryMenu(listeningHistory);
                        break;

                    default:
                        Console.WriteLine("Помилка: невірний вибір. Будь ласка, введіть число від 1 до 6.");
                        break;
                }
            }
        }

        private static void DisplayCurrentTrack(object? sender, TrackStartedEventArgs<Song> e)
        {
            Console.WriteLine($"\n Зараз грає: [Позиція {e.Position + 1}] {e.Track}");
        }

       
        private static void AddSongMenu(Playlist<Song> playlist)
        {
            Console.WriteLine(" ДОДАВАННЯ НОВОГО ТРЕКУ ");

            Console.Write("Введіть назву треку: ");
            string? title = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("Помилка: назва не може бути порожньою.");
                return;
            }

            Console.Write("Введіть виконавця : ");
            string? artist = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(artist))
            {
                artist = "Lana Del Rey";
            }

            Console.Write("Введіть тривалість у секундах: ");
            if (int.TryParse(Console.ReadLine(), out int duration) && duration > 0)
            {
                playlist.Add(new Song(title, artist, duration));
                Console.WriteLine(" Трек успішно додано до плейлиста!");
            }
            else
            {
                Console.WriteLine("Помилка: введено невірне числове значення для тривалості.");
            }
        }


        private static void SelectPlaybackModeMenu(Playlist<Song> playlist)
        {
            Console.WriteLine(" ВИБІР РЕЖИМУ ВІДТВОРЕННЯ ");
            Console.WriteLine("1. По порядку ");
            Console.WriteLine("2. Повтор поточного треку ");
            Console.WriteLine("3. Випадковий порядок ");
            Console.WriteLine("4. У зворотному порядку ");
            Console.Write("Оберіть режим (1-4): ");

            string? modeChoice = Console.ReadLine();
            Console.WriteLine();

            switch (modeChoice)
            {
                case "1":
                    playlist.SetNextTrackRule(null);
                    Console.WriteLine("Режим змінено на: По порядку.");
                    break;
                case "2":
                    playlist.SetNextTrackRule((current, count) => current);
                    Console.WriteLine("Режим змінено на: Повтор треку.");
                    break;
                case "3":
                    playlist.SetNextTrackRule((current, count) => Random.Shared.Next(count));
                    Console.WriteLine("Режим змінено на: Випадковий.");
                    break;
                case "4":
                    playlist.SetNextTrackRule((current, count) => (current - 1 + count) % count);
                    Console.WriteLine("Режим змінено на: У зворотному порядку.");
                    break;
                default:
                    Console.WriteLine("Невірний вибір режиму.");
                    break;
            }
        }

        
        private static void ShowHistoryMenu(List<string> history)
        {
            Console.WriteLine(" ІСТОРІЯ ПРОСЛУХОВУВАННЯ ");
            if (history.Count == 0)
            {
                Console.WriteLine("Історія порожня. Ви ще не перемикали треки.");
                return;
            }

            foreach (var record in history)
            {
                Console.WriteLine(record);
            }
        }
    }
}