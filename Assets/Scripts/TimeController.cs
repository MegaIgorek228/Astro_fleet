using AstroFleet;
using UnityEngine;
using UnityEngine.PlayerLoop;
using TMPro;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // если TMP, иначе using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
    [Header("Настройки времени")]
    private static float speed = 1f;
    public static float globalTime = 0f;
    public static int globalTicks = 0;
    private static bool paused = false;

    [Header("Календарные константы (в секундах)")]
    public float secondsPerMinute = 60f;
    public float secondsPerHour = 3600f;
    public float secondsPerDay = 86400f;          // 24 ч
    public float secondsPerMonth = 30f * 86400f;  // 30 дней
    public float secondsPerYear = 12f * 30f * 86400f; // 12 месяцев = 360 дней

    [Header("UI для отображения")]
    public TextMeshProUGUI timeText;   // время ЧЧ:ММ
    public TextMeshProUGUI dateText;   // дата (день, месяц, год)
    public TextMeshProUGUI pauseText;   // pause
    
    [Header("UI для скорости")]
    public TextMeshProUGUI speedDisplay;
    public Button[] speedButtons;      // кнопки с предустановленными скоростями
    public float[] speedValues;  
    
    

    void Update()
    {
        if (!paused)
        {
            globalTime += dt();
            globalTicks++;
        }

        UpdateTimeDisplay();
        UpdateDateDisplay();
        UpdateSpeedDisplay();
    }

    public static float dt()
    {
        return paused ? 0f : Time.deltaTime * Defines.baseSpeedTime * speed;
    }

    // ----- Методы управления (кнопки) -----
    public void ChangeSpeed(float newSpeed)
    {
        speed = Mathf.Max(0.1f, newSpeed);
        UpdateSpeedDisplay();
    }
    
    private float[] speeds = { 0.1f, 0.7f, 2f, 5f, 11f, 20f };
    private int currentSpeedIndex = 1; // индекс для 0.7f (или 0 для 0.1f)

    public void IncreaseSpeed()
    {
        if (currentSpeedIndex < speeds.Length - 1)
            currentSpeedIndex++;
        speed = speeds[currentSpeedIndex];
    }

    public void DecreaseSpeed()
    {
        if (currentSpeedIndex > 0)
            currentSpeedIndex--;
        speed = speeds[currentSpeedIndex];
    }

    public void TogglePause() { paused = !paused; }
    public void PauseOn()  { paused = true; }
    public void PauseOff() { paused = false; }

    public void Pause()
    {
        if (paused)
        {
            paused = false;
            pauseText.text = "||";
        }
        else
        {
            paused = true;
            pauseText.text = ">";   
        }
    }
    
    private void UpdateSpeedDisplay()
    {
        // 1. Синхронизируем currentSpeedIndex с текущим значением speed
        if (speedValues != null && speedValues.Length > 0)
        {
            int foundIndex = -1;
            for (int i = 0; i < speedValues.Length; i++)
            {
                if (Mathf.Approximately(speed, speedValues[i]))
                {
                    foundIndex = i;
                    break;
                }
            }
            // Если скорость найдена в списке предустановок и индекс изменился — обновляем
            if (foundIndex >= 0 && foundIndex != currentSpeedIndex)
            {
                currentSpeedIndex = foundIndex;
            }
            // Если скорость не найдена, currentSpeedIndex остаётся прежним (кнопки не подсвечиваются)
        }

        // 2. Обновляем текстовый индикатор скорости
        if (speedDisplay != null)
            speedDisplay.text = "x" + speed.ToString("0.0");

        // 3. Подсвечиваем кнопки согласно currentSpeedIndex
        int lastIndex = speedValues.Length - 1;

        for (int i = 0; i < speedButtons.Length && i < speedValues.Length; i++)
        {
            ColorBlock colors = speedButtons[i].colors;

            if (i < currentSpeedIndex)
            {
                colors.normalColor = Color.lightBlue;
            }
            else if (i == currentSpeedIndex)
            {
                colors.normalColor = (currentSpeedIndex == lastIndex) ? Color.mediumVioletRed : Color.lightBlue;
            }
            else // i > currentSpeedIndex
            {
                colors.normalColor = Color.gray;
            }

            speedButtons[i].colors = colors;
        }
    }

    // ----- Обновление времени (ЧЧ:ММ:СС) -----
    private void UpdateTimeDisplay()
    {
        if (timeText == null) return;
        float t = globalTime;
        int hours = Mathf.FloorToInt((t % secondsPerDay) / secondsPerHour);
        int minutes = Mathf.FloorToInt((t % secondsPerHour) / secondsPerMinute);
        int seconds = Mathf.FloorToInt(t % secondsPerMinute);
        timeText.text = string.Format("{0:D2}:{1:D2}", hours, minutes, seconds);
    }

    // ----- Обновление даты (День, Месяц, Год) -----
    private void UpdateDateDisplay()
    {
        if (dateText == null) return;
        float t = globalTime;

        // Вычисляем годы
        int years = Mathf.FloorToInt(t / secondsPerYear);
        float remaining = t % secondsPerYear;

        // Вычисляем месяцы
        int months = Mathf.FloorToInt(remaining / secondsPerMonth);
        remaining -= months * secondsPerMonth;

        // Вычисляем дни
        int days = Mathf.FloorToInt(remaining / secondsPerDay);

        // Приводим к человеческому формату (дни, месяцы, годы начинаются с 1)
        days += 1;
        months += 1;
        years += 1;

        dateText.text = string.Format("{0:D2}.{1:D2}.{2:D2}", days, months, years + 2908);
    }
    public static void SetPaused(bool pause)
        {
            paused = pause;
        }
}