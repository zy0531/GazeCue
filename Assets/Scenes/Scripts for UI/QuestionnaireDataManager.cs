using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class QuestionnaireDataManager : MonoBehaviour
{
    public Slider slider;
    public TMP_Text sliderValueText; // display the slider's value
    public Button button;

    public string participantID;
    public string folderPath;
    public string fileName;
    public string conditionNum;
    public int trialNum;

    /// <summary>
    /// Set the questions here
    /// </summary>
    public List<string> questions; // List of questions
    public TMP_Text textMeshPro_question;
    public List<string> negativeValues; // The most left negative value for each question in the list of questions
    public TMP_Text textMeshPro_negative;
    public List<string> positiveValues; // The most right positive value for each question in the list of questions
    public TMP_Text textMeshPro_positive;
    private int currentIndex = 0; // Index of the current question


    void Start()
    {
        // Set up slider value display
        AddSliderListener();

        // Set up button click event
        AddButtonListener();

        // Display the first question
        DisplayQuestion();
    }


    // **************************** slider ****************************
    void AddSliderListener()
    {
        // Check if the Slider and TextMeshPro components are assigned
        if (slider != null && sliderValueText != null)
        {
            // Update the text initially
            UpdateSliderValueText();

            // Add a listener to the slider to detect value changes
            slider.onValueChanged.AddListener(delegate { UpdateSliderValueText(); });
        }
        else
        {
            Debug.LogWarning("Slider or TextMeshPro reference is not set!");
        }
    }

    public void LogRating()
    {
        if (slider != null)
        {
            float rating = slider.value; // Get the value of the slider
            Debug.Log("Slider value: " + rating); // Log the slider value
        }
        else
        {
            Debug.LogWarning("Slider reference is not set!"); // Log a warning if the slider reference is not set
        }
    }

    // Update the text to display the current slider value
    void UpdateSliderValueText()
    {
        if (sliderValueText != null)
        {
            // Display the current slider value in the TextMeshPro component
            sliderValueText.text = slider.value.ToString("F0"); 
        }
    }


    // **************************** button ****************************
    void AddButtonListener()
    {
        // Check if the Button component is found
        if (button != null)
        {
            // Add an onClick event listener to the button
            button.onClick.AddListener(NextQuestion);
        }
        else
        {
            Debug.LogWarning("Button component not found on GameObject!");
        }
    }

    void NextQuestion()
    {
        // Increment the current index and check if all questions have been displayed
        currentIndex++;
        if (currentIndex >= questions.Count)
        {
            // Disable the button if all questions have been displayed
            button.interactable = false;
        }
        else
        {
            // Display the next question
            DisplayQuestion();
        }
    }

    void DisplayQuestion()
    {
        // Check if the TextMeshPro component is assigned and the questions list is not empty
        if (textMeshPro_question != null && questions != null && questions.Count > 0 && currentIndex < questions.Count)
        {
            // Display the current question
            textMeshPro_question.text = (currentIndex+1).ToString() + ". " + questions[currentIndex];
            // Display the current negative & positive value
            textMeshPro_negative.text = negativeValues[currentIndex];
            textMeshPro_positive.text = positiveValues[currentIndex];
        }
        else
        {
            Debug.LogWarning("TextMeshPro reference is not set or questions list is empty!");
        }
    }
}
