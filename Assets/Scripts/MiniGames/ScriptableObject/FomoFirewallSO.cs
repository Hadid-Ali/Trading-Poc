using UnityEngine;
[CreateAssetMenu(fileName = "FomoSO", menuName = "FomoFirewallData")]
public class FomoFirewallSO : ScriptableObject
{
    [TextArea(2, 3)]
    public string Question;
    [TextArea(2, 3)]
    public string Desc;
    [TextArea(2, 3)]
    public string cmt1;
    [TextArea(2, 3)]
    public string cmt2;
    [TextArea(2, 3)]
    public string cmt3;
    public FomoOption CorrectOption;

    [Header("Feedback")]
    [TextArea(2, 3)]
    public string CorrectWhatHappenDec;
    [TextArea(2, 3)]
    public string CorrectLesson;
    [TextArea(2, 3)]
    public string WrongWhatHappenDec;
    [TextArea(2, 3)]
    public string WrongLesson;
    public Sprite CorrectGraph;
    public Sprite WrongGraph;

}
