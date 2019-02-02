#region

using System;
using System.Collections;
using UnityEngine;

#endregion

public class SuggestionGui : MonoBehaviour
{
    private const string StringEmail = "";

    private SuggestionState _suggestionState = SuggestionState.Write;

    private string _suggestionText = "";

    private Rect _windowRect = new Rect(30, 300, 300, 450);

    private void Start()
    {
        _windowRect.position = new Vector2(Screen.width - 310, 50);
    }

    private IEnumerator SendSuggestion()
    {
        _suggestionState = SuggestionState.Sending;

        var form = new WWWForm();
        form.AddField("email", StringEmail);
        form.AddField("suggestion", _suggestionText);

#pragma warning disable 618
        var www = new WWW("http://squirrelyjones.com/boundingbox/materialize/processSuggestion.php", form);
#pragma warning restore 618
        yield return www;
        var returnText = www.text;
        Debug.Log(www.text);

        if (returnText.Contains("success"))
        {
            _suggestionState = SuggestionState.Sent;
            _suggestionText = "";
        }
        else
        {
            _suggestionState = SuggestionState.Failed;
        }

        yield return new WaitForSeconds(0.01f);
    }

    private void DoMyWindow(int windowId)
    {
        const int offsetX = 10;
        var offsetY = 30;

        switch (_suggestionState)
        {
            case SuggestionState.Write:
            {
                GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Got a suggestion?  Send it to us!");
                offsetY += 30;
                _suggestionText = GUI.TextArea(new Rect(offsetX, offsetY, 280, 170), _suggestionText);
                offsetY += 180;
                if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Send")) StartCoroutine(SendSuggestion());
                if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Close")) gameObject.SetActive(false);
                break;
            }
            case SuggestionState.Sending:
            {
                GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Sending....");
                offsetY += 30;
                if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Close"))
                {
                    _suggestionState = SuggestionState.Write;
                    gameObject.SetActive(false);
                }

                break;
            }
            case SuggestionState.Failed:
            {
                GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Something went wrong!");
                offsetY += 30;
                if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Close"))
                {
                    _suggestionState = SuggestionState.Write;
                    gameObject.SetActive(false);
                }

                if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Try Again"))
                    _suggestionState = SuggestionState.Write;
                break;
            }
            case SuggestionState.Sent:
            {
                GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Thanks!  I'll get right on that!");
                offsetY += 30;
                if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Close"))
                {
                    _suggestionState = SuggestionState.Write;
                    gameObject.SetActive(false);
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        _windowRect.width = 300;
        _windowRect.height = _suggestionState == SuggestionState.Write ? 280 : 100;

        _windowRect = GUI.Window(50, _windowRect, DoMyWindow, "Make A Suggestion!");
    }

    private enum SuggestionState
    {
        Write,
        Sending,
        Failed,
        Sent
    }
}