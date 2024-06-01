using System; // Przestrzeń nazw zawierająca fundamentalne typy i klasy .NET
using System.ComponentModel; // Przestrzeń nazw zawierająca typy, które definiują kontrakty, które obsługują różne aspekty programowania we WPF, takie jak powiadomienia o zmianach, kolekcje i wiele innych
using System.Windows; // Przestrzeń nazw zawierająca klasy dla aplikacji Windows Presentation Foundation (WPF)
using System.Windows.Input; // Przestrzeń nazw zawierająca typy, które obsługują wejście z klawiatury, myszy i innych urządzeń we WPF
using System.Windows.Media.Animation; // Przestrzeń nazw zawiera typy i klasy do obsługi animacji w interfejsie użytkownika
using System.Windows.Media; // Przestrzeń nazw zawiera typy i klasy związane z grafiką i renderowaniem w interfejsie użytkownika
using System.Windows.Threading; // Przestrzeń nazw zawiera klasy i interfejsy związane z programowaniem wielowątkowym i planowaniem zadań w aplikacjach okienkowych platformy Windows
using System.Media; // Przestrzeń nazw zawiera klasy do obsługi multimediów, takie jak odtwarzanie dźwięków i zarządzanie dźwiękiem w aplikacjach
using System.Windows.Controls; // Przestrzeń nazw zawiera definicje wielu podstawowych kontrolek użytkownika, takich jak np. Image
using Newtonsoft.Json; // Przestrzeń nazw, która umożliwia pracę z JSON (serializacja i deserializacja)
using System.Net; // Przestrzeń nazw, która zapewnia klasy dla żądań sieciowych i odpowiedzi
using System.IO; // Przestrzeń nazw, która zapewnia klasy dla operacji wejścia/wyjścia (np. czytanie i pisanie plików)
using System.Windows.Media.Imaging; // Importowanie przestrzeni nazw System.Windows.Media.Imaging, która zapewnia klasy do pracy z obrazami bitmapowymi w aplikacjach WPF
using System.Collections.Generic; // Przestrzeń nazw, która zapewnia klasy dla generycznych kolekcji, takich jak listy, słowniki itp.
using System.Windows.Controls.Primitives; // Przestrzeń nazw, która zapewnia klasy dla elementarnych kontrolek WPF

namespace Pogodynka // Przestrzeń nazw, w której znajdują się klasy i inne elementy związane z aplikacją pogodową
{

    // Główne okno aplikacji, deklaracja klasy częściowo publicznej MainWindow, która dziedziczy po klasie Window, jednocześnie implementując interfejs INotifyPropertyChanged, który powiadamia o zmianach właściwości w klasie
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly string apiKey = "fad66e19814546d2b75155911243105"; // Zmienna przechowująca klucz API do autoryzacji przy korzystaniu z zewnętrznej usługi
        private PogodynkaRestApi forecastData; // Zmienna przechowująca dane pogodowe
        private string cityName; // Zmienna przechowująca nazwę miasta
        private DateTime _now; // Deklaracja prywatnej zmiennej przechowującej aktualny czas
        private ToggleButton _lastCheckedButton; // Zmienna przechowująca odniesienie do ostatnio zaznaczonego przycisku
        private SoundPlayer soundPlayer; // Zmienna używana do odtwarzania plików dźwiękowych

        public MainWindow() // Konstruktor klasy publicznej MainWindow
        {
            InitializeComponent(); // Inicjalizuje składniki okna, które zostały zdefiniowane w pliku XAML
            DataContext = this; // Ustawia bieżące okno jako kontekst danych dla okna

            InitializeControls();
            InitializeTimer();

            txtCityName.PreviewKeyDown += TxtCityName_PreviewKeyDown; // Kiedy użytkownik naciśnie klawisz podczas wprowadzania tekstu w kontrolce, wywołana zostanie ta metoda

            // Uniwersalna metoda obsługująca zdarzenie MouseEnter dla obrazów
            void Image_MouseEnter(object sender, MouseEventArgs e)
            {
                if (sender is Image image)
                {
                    // Tworzymy animację kołysania obrazu
                    DoubleAnimation animation = new DoubleAnimation
                    {
                        From = -10, // Początkowe przesunięcie w lewo
                        To = 10,    // Przesunięcie w prawo
                        Duration = TimeSpan.FromSeconds(0.1), // Czas trwania animacji
                        AutoReverse = true, // Automatyczne powrotna animacji do stanu początkowego
                        RepeatBehavior = RepeatBehavior.Forever // Powtarzamy animację w nieskończoność
                    };

                    // Przypisujemy animację do właściwości TranslateTransform.X obrazu
                    TranslateTransform translateTransform = new TranslateTransform();
                    image.RenderTransform = translateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5); // Punkt transformacji

                    // Uruchamiamy animację
                    translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                }
            }

            // Uniwersalna metoda obsługująca zdarzenie MouseLeave dla obrazów
            void Image_MouseLeave(object sender, MouseEventArgs e)
            {
                if (sender is Image image)
                {
                    // Po opuszczeniu myszą obszaru obrazu zatrzymujemy animację
                    image.RenderTransform.BeginAnimation(TranslateTransform.XProperty, null);
                }
            }

            // Metoda inicjalizująca kontrolki
            void InitializeControls()
            { 
                var daysButtons = new List<ToggleButton> { Day1, Day2, Day3, Day4, Day5, Day6, Day7 };
                foreach (var button in daysButtons)
                {
                    // Powoduje to, że te kontrolki stają się nieaktywne, czyli użytkownik nie może z nimi wchodzić w interakcje
                    button.IsEnabled = false;
                    // Przypisanie zdarzenia kliknięcia w dany dzień do wywołania metody DayButton_Click
                    button.Click += DayButton_Click;
                    // Ustawianie wartości Tag do danego dnia tygodnia
                    button.Tag = daysButtons.IndexOf(button);
                }

                lblDigitalClock.Visibility = Visibility.Hidden; // Powoduje to, że kontrolka staje się niewidoczna, ale wciąż zajmuje miejsce w układzie interfejsu użytkownika

                // Dodajemy obsługę zdarzeń MouseEnter i MouseLeave dla obrazu "Lokalizacja", "Temperatura", "Nawilgocenie", "Wiatr" i "Opady"
                foreach (var image in new List<Image> { Lokalizacja, Temperatura, Nawilgocenie, Wiatr, Opady })
                {
                    image.MouseEnter += Image_MouseEnter;
                    image.MouseLeave += Image_MouseLeave;
                }
            }

            // Metoda, która tworzy i uruchomia timer do odświeżania czasu co sekundę
            void InitializeTimer()
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) }; // Utworzenie obiektu DispatcherTimer
                timer.Tick += (sender, args) => // Przypisanie obsługi zdarzenia Tick dla timera
                {
                    Now = DateTime.Now; // Ustawienie bieżącej daty i czasu w zmiennej Now
                    UpdateDayLabels(); // Wywołanie metody, która aktualizuje etykiety dni
                };
                timer.Start(); // Uruchomienie timera
            }

        }

        // Definicja metody obsługującej zdarzenie dla kontrolki wpisywania nazwy miasta
        private void TxtCityName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Sprawdzanie czy został wprowadzony znak (nie jest to klawisz specjalny)
            if (!e.Key.Equals(Key.Back) && !e.Key.Equals(Key.Delete) && !e.Key.Equals(Key.Enter) && !e.Key.Equals(Key.Left) && !e.Key.Equals(Key.Right))
            {
                SystemSounds.Hand.Play(); // Wydanie dźwięku systemowego
            }
        }

        // Obsługa zdarzenia przeciągania okna myszą, klasa prywatna typu void
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) // Sprawdzenie, czy został naciśnięty lewy przycisk myszy
            {
                this.DragMove(); // Przeciągnięcie okna
            }
        }

        // Obsługa zdarzenia kliknięcia przycisku zamknięcia, klasa prywatna typu void
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Zamknięcie okna
        }

        // Klasa publiczna Now, przechowująca aktualny czas
        public DateTime Now
        {
            get { return _now; } // Zwraca wartość zmiennej _now
            set
            {
                _now = value; // Ustawia wartość zmiennej _now na wartość przypisaną do właściwości Now
                OnPropertyChanged(nameof(Now)); // Wywołuje zdarzenie PropertyChanged, informujące o zmianie wartości właściwości
            }
        }

        // Zdarzenie informujące o zmianie wartości w czasie
        public event PropertyChangedEventHandler PropertyChanged;
        // Metoda wywołująca zdarzenie PropertyChanged, chroniony wirtualny void
        protected virtual void OnPropertyChanged(String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // Wywołanie zdarzenia PropertyChanged, informującego o zmianie wartości właściwości
        }

        private void UpdateDayLabels()
        {
            // Tablica zawierająca skróty nazw dni tygodnia
            var daysOfWeek = new[] { "Pon", "Wt", "Śr", "Czw", "Pt", "Sob", "Nie" };

            // Pętla iterująca przez kolejne 7 dni
            for (int i = 0; i < 7; i++)
            {
                // Pobiera nazwę dnia tygodnia dla dnia aktualnego plus i dni
                var day = Now.AddDays(i).DayOfWeek;

                // Pobiera nazwę dnia tygodnia
                string dayName = GetDayName(day);

                // Ustawia odpowiednie etykiety w zależności od wartości i
                switch (i)
                {
                    case 0:
                        Day1.Content = dayName;
                        Day11.Content = dayName;
                        break;
                    case 1:
                        Day2.Content = dayName;
                        break;
                    case 2:
                        Day3.Content = dayName;
                        break;
                    case 3:
                        Day4.Content = dayName;
                        break;
                    case 4:
                        Day5.Content = dayName;
                        break;
                    case 5:
                        Day6.Content = dayName;
                        break;
                    case 6:
                        Day7.Content = dayName;
                        break;
                }
            }
        }

        private string GetDayName(DayOfWeek dayOfWeek)
        {
            // Zwraca nazwę dnia tygodnia na podstawie wartości DayOfWeek
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday: 
                    return "Pon";
                case DayOfWeek.Tuesday: 
                    return "Wt";
                case DayOfWeek.Wednesday: 
                    return "Śr";
                case DayOfWeek.Thursday: 
                    return "Czw";
                case DayOfWeek.Friday: 
                    return "Pt";
                case DayOfWeek.Saturday: 
                    return "Sob";
                case DayOfWeek.Sunday: 
                    return "Nie";
                default: 
                    return string.Empty;
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Jeśli wcześniej był zaznaczony jakiś przycisk ToggleButton, odznacza go
            if (_lastCheckedButton != null)
            {
                _lastCheckedButton.IsChecked = false;
            }
            // Ustawia aktualnie kliknięty przycisk jako ostatnio zaznaczony
            _lastCheckedButton = sender as ToggleButton;
            _lastCheckedButton.IsChecked = true;
        }

        // Metoda pomocnicza do ustawiania stanu przycisku dnia
        private void SetDayButtonState(ToggleButton button, bool isChecked, bool isEnabled)
        {
            button.IsChecked = isChecked;
            button.IsEnabled = isEnabled;
        }

        // Metoda pomocnicza do wyłączania wszystkich przycisków dnia
        private void DisableAllDayButtons()
        {
            foreach (var button in new List<ToggleButton> { Day1, Day2, Day3, Day4, Day5, Day6, Day7 })
            {
                button.IsEnabled = false;
            }
        }

        // Metoda pomocnicza do resetowania stanu przycisków dnia
        private void ResetDayButtons()
        {
            _lastCheckedButton = null;
            foreach (var button in new List<ToggleButton> { Day1, Day2, Day3, Day4, Day5, Day6, Day7 })
            {
                button.IsChecked = false;
                button.IsEnabled = true;
            }
        }

        // Metoda obsługująca zdarzenie przycisku w dany dzień, pokazuje na dany dzień parametry pogodowe
        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                // Resetuj stan wszystkich przycisków przed zaznaczeniem wybranego dnia
                ResetDayButtons();
                SetDayButtonState(button, true, true);

                int dayIndex = int.Parse(button.Tag.ToString()); // Odczytuje wartość nazwy dnia z przycisku i zamienia na liczbe, która reprezentuje dzień
                ShowWeatherForDay(dayIndex); // Pokazuje dane pogodowe na inne dni
            }
        }

        // Obsługa zdarzenia kliknięcia przycisku pobrania pogody, klasa prywatna typu void asynchroniczna
        private async void btnGetWeather_Click(object sender, RoutedEventArgs e)
        {
            SystemSounds.Beep.Play(); // Wydanie dźwięku systemowego

            // Ustaw Day1 jako zaznaczony oraz domyślny i resetuj pozostałe
            ResetDayButtons();
            SetDayButtonState(Day1, true, true);

            // Usuwa białe znaki i konwertuje tekst tak, aby można było pobrać dane z miasta z Rest API
            cityName = txtCityName.Text.Trim();

            // Jeśli pole z nazwą miasta jest puste
            if (string.IsNullOrEmpty(cityName) )
            {
                DisableAllDayButtons();
                MessageBox.Show("Proszę wprowadzić nazwę miasta!");
                return;
            }

            // Tworzy URL do API pogody, wykorzystując klucz API i nazwę miasta wprowadzone przez użytkownika, pogoda na 7 dni
            string apiUrl = $"http://api.weatherapi.com/v1/forecast.json?key={apiKey}&q={cityName}&days=7";

            try
            {
                // Tworzy żądanie HTTP do powyższego URL
                HttpWebRequest request = WebRequest.CreateHttp(apiUrl);

                // Ustawia metodę HTTP na GET, co oznacza, że chcemy pobrać dane
                request.Method = "GET";

                // Wysyła żądanie i czeka na odpowiedź asynchronicznie
                using (WebResponse response = await request.GetResponseAsync())
                {
                    // Otwiera strumień odpowiedzi
                    using (Stream stream = response.GetResponseStream())
                    {
                        // Tworzy obiekt StreamReader do czytania strumienia odpowiedzi
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            // Czyta całą odpowiedź jako string
                            string jsonResponse = reader.ReadToEnd();

                            // Deserializuje odpowiedź JSON na obiekt typu PogodynkaRestApi
                            PogodynkaRestApi pogodynkaRestApi = JsonConvert.DeserializeObject<PogodynkaRestApi>(jsonResponse);

                            // Wywołuje metodę, która wyświetla dane pogodowe w interfejsie użytkownika
                            PokazPogodynkaRestApi(pogodynkaRestApi);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                // Jeśli wystąpił błąd podczas pobierania danych, wyświetla komunikat z opisem błędu
                MessageBox.Show("Znaleziono błąd podczas pobierania pogody: " + ex.Message);
            }
            // Ustawia widoczność etykiety zegara cyfrowego na widoczną
            lblDigitalClock.Visibility = Visibility.Visible;

            // Czyści pole tekstowe nazwy miasta
            txtCityName.Text = "";

        }

        // Metoda wyświetlająca dane pogodowe w interfejsie użytkownika
        private void PokazPogodynkaRestApi(PogodynkaRestApi pogodynkaRestApi)
        {
            forecastData = pogodynkaRestApi;
            ShowWeatherForDay(0); // Pokaż dane dla pierwszego dnia domyślnie

            // Ustawia zawartość etykiet interfejsu użytkownika na podstawie danych pogodowych
            lblCityName.Content = pogodynkaRestApi.Location.Name;
            lblCondition.Content = pogodynkaRestApi.Current.Condition.Text;
            lblTemperature.Content = pogodynkaRestApi.Current.TempC + " °C";
            lblHumidity.Content = pogodynkaRestApi.Current.Humidity + " %";
            lblWindSpeed.Content = pogodynkaRestApi.Current.WindKph + " km/h";
            lblPrecipitation.Content = pogodynkaRestApi.Current.Precipitation + " mm";

            // Lista obiektów Image reprezentujących ikony pogody dla dni tygodnia
            var forecastIcons = new List<Image> { imgWeatherIconMonday, imgWeatherIconTuesday, imgWeatherIconWednesday, imgWeatherIconThursday, imgWeatherIconFriday, imgWeatherIconSaturday, imgWeatherIconSunday };

            // Przechodzi przez listę ikon pogodowych i ustawia źródło obrazu na podstawie danych prognozy
            for (int i = 0; i < forecastIcons.Count; i++)
            {
                if (i < pogodynkaRestApi.Forecast.ForecastDay.Count)
                {
                    // Tworzy obiekt BitmapImage dla ikony pogody
                    BitmapImage weatherIcon = new BitmapImage();
                    weatherIcon.BeginInit();

                    // Ustawia źródło URI obrazu ikony pogody
                    weatherIcon.UriSource = new Uri("http:" + pogodynkaRestApi.Forecast.ForecastDay[i].Day.Condition.Icon);
                    weatherIcon.EndInit();

                    // Ustawia źródło obrazu w kontrolce Image
                    forecastIcons[i].Source = weatherIcon;
                }
            }
        }

        // Metoda wyświetlająca prognozę pogody dla określonego dnia
        private void ShowWeatherForDay(int dayIndex)
        {
            if (forecastData != null && dayIndex >= 0 && dayIndex < forecastData.Forecast.ForecastDay.Count) // Sprawdza zakres dni
            {
                var forecastForDay = forecastData.Forecast.ForecastDay[dayIndex]; // Pobiera prognozę
                lblCityName.Content = forecastData.Location.Name;
                lblCondition.Content = forecastForDay.Day.Condition.Text;
                lblTemperature.Content = forecastForDay.Day.AvgTempC + " °C";
                lblHumidity.Content = forecastForDay.Day.AvgHumidity + " %";
                lblWindSpeed.Content = forecastForDay.Day.MaxWindKph + " km/h";
                lblPrecipitation.Content = forecastForDay.Day.Precipitation + " mm";
            }
        }
    }
}