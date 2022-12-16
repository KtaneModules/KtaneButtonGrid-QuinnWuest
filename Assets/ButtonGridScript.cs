using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class ButtonGridScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;
    public KMColorblindMode ColorblindMode;

    public KMSelectable[] ButtonSels;
    public GameObject[] ButtonObjs;
    public TextMesh[] CBTexts;
    public KMSelectable ResetSel;
    public GameObject[] LedObjs;
    public Material[] ButtonMats;
    public Material[] LedMats;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;

    private bool _colorblindMode;
    private ButtonColor[] _buttonColors;
    private List<ButtonColor> _expectedAnswers;
    private readonly List<int> _totalPresses = new List<int>();
    private readonly List<int> _currentPresses = new List<int>();
    private bool _unicorn;
    private bool _sixtyNineTheSexNumber;

    public enum ButtonColor
    {
        Red,
        Yellow,
        Green,
        Blue
    }

    private void Start()
    {
        _colorblindMode = ColorblindMode.ColorblindModeActive;
        SetColorblindMode(_colorblindMode);
        _moduleId = _moduleIdCounter++;
        for (int i = 0; i < ButtonSels.Length; i++)
            ButtonSels[i].OnInteract += ButtonPress(i);
        ResetSel.OnInteract += ResetPress;
        Generate();
        if (BombInfo.GetOnIndicators().Contains("BOB") && BombInfo.GetPorts().Contains("DVI") && BombInfo.GetBatteryCount() == 1 && BombInfo.GetSerialNumber().Any(i => i == 'C' || i == 'H' || i == 'R' || i == 'I' || i == 'S'))
        {
            Debug.LogFormat("[Button Grid #{0}] The unicorn rule applies.", _moduleId);
            _unicorn = true;
        }
    }

    private void SetColorblindMode(bool mode)
    {
        foreach (var t in CBTexts)
            t.gameObject.SetActive(mode);
    }

    private KMSelectable.OnInteractHandler ButtonPress(int btn)
    {
        return delegate ()
        {
            ButtonSels[btn].AddInteractionPunch(0.35f);
            Audio.PlaySoundAtTransform("ButtonPress", ButtonSels[btn].transform);
            if (_moduleSolved)
                return false;
            _currentPresses.Add(btn);
            if (_currentPresses.Count == 4)
            {
                if (_totalPresses.Count() == 0 && _unicorn && _buttonColors[_currentPresses[0]] == ButtonColor.Blue && _buttonColors[_currentPresses[1]] == ButtonColor.Red && _buttonColors[_currentPresses[2]] == ButtonColor.Blue && _buttonColors[_currentPresses[3]] == ButtonColor.Yellow && _currentPresses.Distinct().Count() == 4)
                {
                    _moduleSolved = true;
                    Module.HandlePass();
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    Debug.LogFormat("[Button Grid #{0}] The unicorn rule applied. Module solved!", _moduleId);
                    for (int i = 0; i < 20; i++)
                    {
                        CBTexts[i].text = "";
                        ButtonObjs[i].GetComponent<MeshRenderer>().material = ButtonMats[(int)ButtonColor.Green];
                    }
                    if (_sixtyNineTheSexNumber)
                        FourHundredTwentyBlazeIt();
                    return false;
                }
                var correct = Enumerable.Range(0, 4).All(x => _expectedAnswers[_totalPresses.Count + x] == _buttonColors[_currentPresses[x]]);
                var validPos = _currentPresses.All(i => !_totalPresses.Contains(i));
                if (correct && validPos)
                {
                    Debug.LogFormat("[Button Grid #{0}] Correctly pressed {1}.", _moduleId, _currentPresses.Select(i => _buttonColors[i].ToString() + " (" + (i + 1) + ")").Join(", "));
                    _totalPresses.AddRange(_currentPresses);
                    for (int i = 0; i < _totalPresses.Count / 4; i++)
                        LedObjs[i].GetComponent<MeshRenderer>().material = LedMats[1];
                    if (_totalPresses.Count == 20)
                    {
                        _moduleSolved = true;
                        Module.HandlePass();
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                        Debug.LogFormat("Button Grid #{0}] Module solved.", _moduleId);
                        for (int i = 0; i < 20; i++)
                        {
                            CBTexts[i].text = "";
                            ButtonObjs[i].GetComponent<MeshRenderer>().material = ButtonMats[(int)ButtonColor.Green];
                        }
                        if (_sixtyNineTheSexNumber)
                            FourHundredTwentyBlazeIt();
                    }
                }
                else
                {
                    Module.HandleStrike();
                    Debug.LogFormat("[Button Grid #{0}] Incorrectly pressed {1}. Strike.", _moduleId, _currentPresses.Select(i => _buttonColors[i].ToString() + " (" + (i + 1) + ")").Join(", "));
                }
                _currentPresses.Clear();
            }
            return false;
        };
    }

    private bool ResetPress()
    {
        ResetSel.AddInteractionPunch(0.5f);
        Audio.PlaySoundAtTransform("ButtonPress", ResetSel.transform);
        if (_moduleSolved)
            FourHundredTwentyBlazeIt();
        else
        {
            for (int i = 0; i < 5; i++)
                LedObjs[i].GetComponent<MeshRenderer>().material = LedMats[0];
            _totalPresses.Clear();
            _currentPresses.Clear();
        }
        return false;
    }

    private void FourHundredTwentyBlazeIt()
    {
        var bakersdozenbagels = new int[20] { 4, 0, 0, 0, 4, 0, 0, 3, 3, 4, 0, 0, 0, 0, 4, 4, 0, 4, 0, 4 };
        for (int i = 0; i < 20; i++)
            ButtonObjs[i].GetComponent<MeshRenderer>().material = ButtonMats[bakersdozenbagels[i]];
    }

    private void Generate()
    {
        _buttonColors = Enumerable.Range(0, 20).Select(i => (ButtonColor)(i / 5)).ToArray().Shuffle();
        for (int i = 0; i < _buttonColors.Length; i++)
        {
            ButtonObjs[i].GetComponent<MeshRenderer>().sharedMaterial = ButtonMats[(int)_buttonColors[i]];
            CBTexts[i].text = _buttonColors[i].ToString().Substring(0, 1);
        }
        _expectedAnswers = new List<ButtonColor>();

        // Stage 1
        Debug.LogFormat("[Button Grid #{0}] Stage 1:", _moduleId);
        if (new int[4] { 0, 4, 15, 19 }.Select(i => _buttonColors[i]).Distinct().Count() == 4)
        {
            Debug.LogFormat("[Button Grid #{0}] The four corners are different colors. Adding: Red, Blue, Yellow, Green.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Red, ButtonColor.Blue, ButtonColor.Yellow, ButtonColor.Green });
        }
        else if (Enumerable.Range(0, 5).Select(i => new[] { _buttonColors[i], _buttonColors[i + 5], _buttonColors[i + 10], _buttonColors[i + 15] }).Any(i => i.Distinct().Count() == 4))
        {
            Debug.LogFormat("[Button Grid #{0}] A column contains all four colors. Adding: Green, Red, Yellow, Blue.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Green, ButtonColor.Red, ButtonColor.Yellow, ButtonColor.Blue });
        }
        else if (Enumerable.Range(0, 4).Select(i => new[] { _buttonColors[i * 5], _buttonColors[i * 5 + 1], _buttonColors[i * 5 + 2], _buttonColors[i * 5 + 3], _buttonColors[i * 5 + 4] }).Any(i => i.Distinct().Count() != 4))
        {
            Debug.LogFormat("[Button Grid #{0}] A row is missing a color entirely. Adding: Red, Yellow, Blue, Green.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Red, ButtonColor.Yellow, ButtonColor.Blue, ButtonColor.Green });
        }
        else
        {
            Debug.LogFormat("[Button Grid #{0}] None of the rules for this stage applied. Adding: Green, Yellow, Blue, Red.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Green, ButtonColor.Yellow, ButtonColor.Blue, ButtonColor.Red });
        }
        // Stage 2
        Debug.LogFormat("[Button Grid #{0}] Stage 2:", _moduleId);
        if (Enumerable.Range(0, 10).Where(i => _buttonColors[i] == ButtonColor.Blue).Count() == 5)
        {
            Debug.LogFormat("[Button Grid #{0}] The top half of the grid contains all of the blue buttons. Adding: Blue, Red, Yellow, Green.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Blue, ButtonColor.Red, ButtonColor.Yellow, ButtonColor.Green });
        }
        else if (Enumerable.Range(10, 10).Where(i => _buttonColors[i] == ButtonColor.Red).Count() >= 3)
        {
            Debug.LogFormat("[Button Grid #{0}] The bottom half contains at least three red buttons. Adding: Red, Green, Yellow, Blue.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Red, ButtonColor.Green, ButtonColor.Yellow, ButtonColor.Blue });
        }
        else if (Enumerable.Range(0, 20).Any(i => _buttonColors[i] == ButtonColor.Green && ((i % 5 != 0 && _buttonColors[i - 1] == ButtonColor.Green) || (i % 5 != 4 && _buttonColors[i + 1] == ButtonColor.Green) || (i / 5 != 0 && _buttonColors[i - 5] == ButtonColor.Green) || (i / 5 != 3 && _buttonColors[i + 5] == ButtonColor.Green))))
        {
            Debug.LogFormat("[Button Grid #{0}] There is a green button adjacent to another green button. Adding: Green, Blue, Red, Yellow.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Green, ButtonColor.Blue, ButtonColor.Red, ButtonColor.Yellow });
        }
        else
        {
            Debug.LogFormat("[Button Grid #{0}] None of the rules for this stage applied. Adding: Yellow, Red, Blue, Green.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Yellow, ButtonColor.Red, ButtonColor.Blue, ButtonColor.Green });
        }
        // Stage 3
        Debug.LogFormat("[Button Grid #{0}] Stage 3:", _moduleId);
        if (BombInfo.GetSerialNumber().Any(i => i == 'B' || i == 'T' || i == 'N'))
        {
            Debug.LogFormat("[Button Grid #{0}] The bomb's serial number contains B, T, or N. Adding: Green, Blue, Red, Yellow.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Green, ButtonColor.Blue, ButtonColor.Red, ButtonColor.Yellow });
        }
        else if (BombInfo.GetSerialNumber().Any(i => i == 'G' || i == 'R' || i == 'D'))
        {
            Debug.LogFormat("[Button Grid #{0}] The bomb's serial number contains G, R, or D. Adding: Red, Green, Blue, Yellow.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Red, ButtonColor.Green, ButtonColor.Blue, ButtonColor.Yellow });
        }
        else if (BombInfo.GetSerialNumber().Any(i => i == 'A' || i == 'E' || i == 'I' || i == 'O' || i == 'U'))
        {
            Debug.LogFormat("[Button Grid #{0}] The bomb's serial number contains a vowel. Adding: Blue, Green, Red, Yellow.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Blue, ButtonColor.Green, ButtonColor.Red, ButtonColor.Yellow });
        }
        else
        {
            Debug.LogFormat("[Button Grid #{0}] None of the rules for this stage applied. Adding: Yellow, Red, Green, Blue.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Yellow, ButtonColor.Red, ButtonColor.Green, ButtonColor.Blue });
        }
        // Stage 4
        Debug.LogFormat("[Button Grid #{0}] Stage 4:", _moduleId);
        if (_buttonColors[0] == ButtonColor.Red)
        {
            Debug.LogFormat("[Button Grid #{0}] The first button's color is red. Adding: Red, Yellow, Green, Blue.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Red, ButtonColor.Yellow, ButtonColor.Green, ButtonColor.Blue });
        }
        else if (_buttonColors[0] == ButtonColor.Green)
        {
            Debug.LogFormat("[Button Grid #{0}] The first button's color is red. Adding: Green, Red, Blue, Yellow.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Green, ButtonColor.Red, ButtonColor.Blue, ButtonColor.Yellow });
        }
        else if (_buttonColors[0] == ButtonColor.Yellow)
        {
            Debug.LogFormat("[Button Grid #{0}] The first button's color is red. Adding: Yellow, Red, Green, Blue.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Yellow, ButtonColor.Red, ButtonColor.Green, ButtonColor.Blue });
        }
        else
        {
            Debug.LogFormat("[Button Grid #{0}] None of the rules for this stage applied. Adding: Blue, Red, Yellow, Green.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Blue, ButtonColor.Red, ButtonColor.Yellow, ButtonColor.Green });
        }
        // Stage 5
        Debug.LogFormat("[Button Grid #{0}] Stage 5:", _moduleId);
        if (_buttonColors[19] == ButtonColor.Yellow)
        {
            Debug.LogFormat("[Button Grid #{0}] The last button's color is yellow. Adding: Red, Blue, Green, Yellow", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Red, ButtonColor.Blue, ButtonColor.Green, ButtonColor.Yellow });
        }
        else if (_buttonColors[19] == ButtonColor.Green)
        {
            Debug.LogFormat("[Button Grid #{0}] The last button's color is green. Adding: Blue, Yellow, Red, Green", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Blue, ButtonColor.Yellow, ButtonColor.Red, ButtonColor.Green });
        }
        else if (_buttonColors[19] == ButtonColor.Blue)
        {
            Debug.LogFormat("[Button Grid #{0}] The last button's color is blue. Adding: Green, Yellow, Red, Blue", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Green, ButtonColor.Yellow, ButtonColor.Red, ButtonColor.Blue });
        }
        else
        {
            Debug.LogFormat("[Button Grid #{0}] None of the rules for this stage applied. Adding: Yellow, Blue, Green, Red.", _moduleId);
            _expectedAnswers.AddRange(new ButtonColor[] { ButtonColor.Yellow, ButtonColor.Blue, ButtonColor.Green, ButtonColor.Red });
        }
    }

#pragma warning disable 0414
    private readonly string TwitchHelpMessage = "!{0} press 1 2 3 4 [Press buttons 1, 2, 3, 4.] | Buttons are labeled in reading order. | !{0} reset [Press the reset button.]";
#pragma warning restore 0414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.Match(command, @"^\s*sus(sy|picious)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Success)
        {
            yield return null;
            yield return "sendtochaterror Suspicious command.";
            _sixtyNineTheSexNumber = true;
            yield break;
        }
        if (Regex.Match(command, @"^\s*reset\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Success)
        {
            yield return null;
            ResetSel.OnInteract();
            yield break;
        }
        command = command.Trim().ToLowerInvariant();
        if (!command.StartsWith("press "))
            yield break;
        command = command.Substring(6);
        var parameters = command.Split(' ');
        var btnsNumber = new List<int>();
        var fine = true;
        for (int i = 0; i < parameters.Length; i++)
        {
            int val;
            if (!int.TryParse(parameters[i], out val) || val < 1 || val > 20)
            {
                fine = false;
                break;
            }
            else
                btnsNumber.Add(val - 1);
        }
        if (fine)
        {
            yield return null;
            for (int i = 0; i < btnsNumber.Count; i++)
            {
                ButtonSels[btnsNumber[i]].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            yield break;
        }
        var btnsCoords = new List<int>();
        for (int i = 0; i < parameters.Length; i++)
        {
            int col = "abcde".IndexOf(parameters[i][0]);
            int row = "1234".IndexOf(parameters[i][1]);
            if (col == -1 || row == -1)
                yield break;
            btnsCoords.Add(row * 5 + col);
        }
        yield return null;
        for (int i = 0; i < btnsCoords.Count; i++)
        {
            ButtonSels[btnsCoords[i]].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }

    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        ResetSel.OnInteract();
        yield return new WaitForSeconds(0.1f);
        var list = new List<int>();
        var btns = _buttonColors.ToList();
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int ix = btns.IndexOf(_expectedAnswers[i * 4 + j]);
                list.Add(ix);
                btns[ix] = (ButtonColor)4;
            }
        }
        for (int i = 0; i < list.Count; i++)
        {
            ButtonSels[list[i]].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
