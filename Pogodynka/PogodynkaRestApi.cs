using System.Collections.Generic; // Przestrzeń nazw, która zapewnia klasy dla generycznych kolekcji, takich jak listy, słowniki itp.
using Newtonsoft.Json; // Przestrzeń nazw, która umożliwia pracę z JSON (serializacja i deserializacja)

namespace Pogodynka // Przestrzeń nazw, w której znajdują się klasy i inne elementy związane z aplikacją pogodową
{
    // Klasa reprezentująca lokalizację (miasto) w danych pogodowych
    public class Location
    {
        // Właściwość mapująca nazwę miasta z JSONa
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    // Klasa reprezentująca warunki pogodowe
    public class Condition
    {
        // Właściwość mapująca opis warunków pogodowych (np. "Słonecznie")
        [JsonProperty("text")]
        public string Text { get; set; }

        // Właściwość mapująca ikonę warunków pogodowych (URL do obrazka)
        [JsonProperty("icon")]
        public string Icon { get; set; }
    }

    // Klasa reprezentująca prognozę pogody na dany dzień
    public class ForecastDay
    {
        // Właściwość mapująca datę prognozy
        [JsonProperty("date")]
        public string Date { get; set; }

        // Właściwość mapująca dane pogodowe na dany dzień
        [JsonProperty("day")]
        public Day Day { get; set; }
    }

    // Główna klasa reprezentująca odpowiedź API pogody
    public class PogodynkaRestApi
    {
        // Właściwość mapująca lokalizację (miasto)
        [JsonProperty("location")]
        public Location Location { get; set; }

        // Właściwość mapująca aktualne dane pogodowe
        [JsonProperty("current")]
        public Current Current { get; set; }

        // Właściwość mapująca prognozę pogody
        [JsonProperty("forecast")]
        public Forecast Forecast { get; set; }
    }

    // Klasa reprezentująca aktualne dane pogodowe
    public class Current
    {
        // Właściwość mapująca bieżące warunki pogodowe
        [JsonProperty("condition")]
        public Condition Condition { get; set; }

        // Właściwość mapująca bieżącą temperaturę w stopniach Celsjusza
        [JsonProperty("temp_c")]
        public double TempC { get; set; }

        // Właściwość mapująca bieżącą prędkość wiatru w km/h
        [JsonProperty("wind_kph")]
        public double WindKph { get; set; }

        // Właściwość mapująca bieżącą wilgotność w procentach
        [JsonProperty("humidity")]
        public double Humidity { get; set; }

        // Właściwość mapująca bieżące opady w mm
        [JsonProperty("precip_mm")]
        public double Precipitation { get; set; }
    }

    // Klasa reprezentująca prognozę pogody zawierającą listę prognoz na kolejne dni
    public class Forecast
    {
        // Właściwość mapująca listę prognoz na kolejne dni
        [JsonProperty("forecastday")]
        public List<ForecastDay> ForecastDay { get; set; }
    }

    // Klasa reprezentująca dane pogodowe na inne dni
    public class Day
    {
        // Właściwość mapująca warunki pogodowe na dany dzień
        [JsonProperty("condition")]
        public Condition Condition { get; set; }

        // Właściwość mapująca średnią temperaturę w ciągu dnia w stopniach Celsjusza
        [JsonProperty("avgtemp_c")]
        public double AvgTempC { get; set; }

        // Właściwość mapująca maksymalną prędkość wiatru w km/h w ciągu dnia
        [JsonProperty("maxwind_kph")]
        public double MaxWindKph { get; set; }

        // Właściwość mapująca średnią wilgotność w ciągu dnia w procentach
        [JsonProperty("avghumidity")]
        public double AvgHumidity { get; set; }

        // Właściwość mapująca średnie opady w mm
        [JsonProperty("totalprecip_mm")]
        public double Precipitation { get; set; }
    }
}