namespace AstroFleet
{
    public class Defines
    {
        public const float G = 6.67e-11f;                // Грав. постоянная
        public const float DistScale = 5e10f;            // 1 юнити единица в метрах
        public const float baseSpeedTime = 60f*30f;      // сколько секунд за 1 секунду игры
        public const float LightSpeed = 299792458f;      // Скорость света
        
        public const float SIT = 5f;                   // Раз в сколько тиков охраняется позиция кораблей
        // public const int DSS = 500;                      // Сколько точек пути сохраняется в памяти на корабль
        public const float EPSD = 0.02f;                // Погрешность в растоянии юнити единицы
        public const float EPSA = 2f;                    // Погрешность в углах градусы
        public const float EPSS = 1e-12f;            // Погрешность скорости
        
    }
}