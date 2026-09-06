using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    public void LoadGameScene()
    {
        // Загружаем сцену по индексу 1 (или по имени)
        SceneManager.LoadScene("SampleScene");
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
    public void ExitGame()
    {
        // В редакторе это не сработает, но в собранной игре закроет приложение
        Application.Quit();
        
        // Для отладки в редакторе выведем сообщение в консоль
        Debug.Log("Выход из игры");
    }
    
}