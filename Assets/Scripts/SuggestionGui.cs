using System.Collections;
using UnityEngine;

public class SuggestionGui : MonoBehaviour
{
    public GameObject AuthenticateObject;

    public GameObject MainGuiObject;
    private MainGui MainGuiScript;
    private bool sendingSuggestion = false;
    private readonly string stringEmail = "";

    private bool suggestionSent = false;

    private SuggestionState suggestionState = SuggestionState.Write;
//	AuthenticateGui AuthenticateScript;

    private string SuggestionText = "";

    private Rect windowRect = new Rect(30, 300, 300, 450);

    // Use this for initialization
    private void Start()
    {
//		AuthenticateScript = AuthenticateObject.GetComponent<AuthenticateGui> ();
//		stringEmail = AuthenticateScript.stringEmail;
        Debug.Log("Suggestion Box Email: " + stringEmail);
        windowRect.position = new Vector2(Screen.width - 310, 50);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private IEnumerator SendSuggestion()
    {
        suggestionState = SuggestionState.Sending;

        // Create a Web Form
        var form = new WWWForm();
        form.AddField("email", stringEmail);
        form.AddField("suggestion", SuggestionText);

        //WWW www = new WWW("http://boundingboxsoftware.com/materialize/processSuggestion.php", form);
        var www = new WWW("http://squirrelyjones.com/boundingbox/materialize/processSuggestion.php", form);
        yield return www;
        var returnText = www.text;
        Debug.Log(www.text);

        if (returnText.Contains("success"))
        {
            suggestionState = SuggestionState.Sent;
            SuggestionText = "";
        }
        else
        {
            suggestionState = SuggestionState.Failed;
        }

        yield return new WaitForSeconds(0.01f);
    }

    private void DoMyWindow(int windowID)
    {
        var offsetX = 10;
        var offsetY = 30;

        if (suggestionState == SuggestionState.Write)
        {
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Got a suggestion?  Send it to us!");
            offsetY += 30;
            SuggestionText = GUI.TextArea(new Rect(offsetX, offsetY, 280, 170), SuggestionText);
            offsetY += 180;
            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Send")) StartCoroutine(SendSuggestion());
            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Close")) gameObject.SetActive(false);
        }
        else if (suggestionState == SuggestionState.Sending)
        {
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Sending....");
            offsetY += 30;
            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Close"))
            {
                suggestionState = SuggestionState.Write;
                gameObject.SetActive(false);
            }
        }
        else if (suggestionState == SuggestionState.Failed)
        {
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Something went wrong!");
            offsetY += 30;
            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Close"))
            {
                suggestionState = SuggestionState.Write;
                gameObject.SetActive(false);
            }

            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Try Again"))
                suggestionState = SuggestionState.Write;
        }
        else if (suggestionState == SuggestionState.Sent)
        {
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Thanks!  I'll get right on that!");
            offsetY += 30;
            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Close"))
            {
                suggestionState = SuggestionState.Write;
                gameObject.SetActive(false);
            }
        }

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        windowRect.width = 300;
        if (suggestionState == SuggestionState.Write)
            windowRect.height = 280;
        else
            windowRect.height = 100;

        windowRect = GUI.Window(50, windowRect, DoMyWindow, "Make A Suggestion!");
    }

    private enum SuggestionState
    {
        Write,
        Sending,
        Failed,
        Sent
    }
}