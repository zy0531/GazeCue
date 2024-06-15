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

    /// <summary>
    /// Set the questions here
    /// </summary>
    public List<string> questions; // List of questions
    public TMP_Text textMeshPro_question;
    public List<string> negativeValues; // The most left negative value for each question in the list of questions
    public TMP_Text textMeshPro_negative;
    public List<string> positiveValues; // The most right positive value for each question in the list of questions
    public TMP_Text textMeshPro_positive;
    public int currentIndex { get; set; } // Index of the current question
    public bool isQuestionnaireFinish { get; set; }


    /// <summary>
    /// Data Recording
    /// </summary>
    [SerializeField] DataManager dataManager;
    string Path;
    string FileName;

    [SerializeField] GazePilotExperimentManager gazePilotExperimentManager;
    [SerializeField] GazeCue gazeCue;

    void Start()
    {
        // Default currentIndex
        currentIndex = 0;

        // Set up slider value display
        AddSliderListener();

        // Set up button click event
        AddButtonListener();

        // Display the first question
        DisplayQuestion(currentIndex);

        // Record Data - "QuestionnaireResponse"
        Path = dataManager.folderPath;
        FileName = "QuestionnaireResponse";
        RecordData.SaveData(Path, FileName,
          "trialID_accumulated" + ","
          + "DisplayMechanism" + ","
          + "DwellTimeThreshold_Near" + ","
          + "DwellTimeThreshold_Far" + ","
          + "QuestionID" + ","
          + "QuestionAnswer" + ","
          + '\n');
    }


    public void ResetQuestionIndex()
    {
        currentIndex = 0;
        // Reset questionnaire finish status
        isQuestionnaireFinish = false;
        // Enable the button when the question index reset
        button.interactable = true;
        // Reset the slider value
        ResetSliderValue();
        // Display the 1st question by default
        DisplayQuestion(currentIndex);
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

    // Reset the slider value to 0
    void ResetSliderValue()
    {
        if (slider != null)
        {
            slider.value = 0; // Get the value of the slider
        }
        else
        {
            Debug.LogWarning("Slider reference is not set!");
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
        // Record the answer of previous question
        LogQuestionResponse();

        // Increment the current index
        currentIndex++;
        // Check if all questions have been displayed    
        if (currentIndex >= questions.Count)
        {
            isQuestionnaireFinish = true;
            // Disable the button if all questions have been displayed
            button.interactable = false;
        }
        else
        {
            // Display the question
            DisplayQuestion(currentIndex);
            // Reset the slider value
            ResetSliderValue();
        }
    }

    void DisplayQuestion(int index)
    {
        // Check if the TextMeshPro component is assigned and the questions list is not empty
        if (textMeshPro_question != null && questions != null && questions.Count > 0 && index < questions.Count)
        {
            // Display the current question
            textMeshPro_question.text = (index + 1).ToString() + ". " + questions[index];
            // Display the current negative & positive value
            textMeshPro_negative.text = negativeValues[index];
            textMeshPro_positive.text = positiveValues[index];
        }
        else
        {
            Debug.LogWarning("TextMeshPro reference is not set or questions list is empty!");
        }
    }

    void LogQuestionResponse()
    {
        RecordData.SaveData(Path, FileName,
          gazePilotExperimentManager.trialID_accumulated + ","
          + gazePilotExperimentManager.displayMechanismType + ","
          + gazeCue.dwellTimeThreshold_near + ","
          + gazeCue.dwellTimeThreshold_far + ","
          + currentIndex + ","
          + slider.value + ","
          + '\n');
    }


}
