using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {

    #region Variables
    public enum MenuType {Game, Start, WrongAnswer, RightAnswer, BuySkips, BuyRemoves, BuyReveals, NextLevel};
	
	private MenuType _currentMenu = MenuType.Start;
	private MenuType currentMenu {
	
		get {return _currentMenu; }
		set {
			
			_currentMenu = value;
			
			switch (_currentMenu) {
			case MenuType.RightAnswer:
				rightAnswerTexture = rightAnswerTextures[Random.Range(0, rightAnswerTextures.Count)];
				break;
			}
			
		}
		
	}

    public GameObject StartButton;
    public GameObject NextLevelButton;

    int currentMission = 1;
	int currentLevel = 1;
	TextAsset missions;
	Texture2D texture;
	Parser parse;
	
	bool wrong = false;
	
	private float imageSizeX = 0.478f, imageSizeY = 0.22f, imagePosY = 0.19052f, imagePosX = 0.5f;
	private float moveUp = - Screen.height * 100/768f;
	
	int currentAnswerLength;
	int[] letterIndex;
	
	int helpDelete {
		get {return PlayerPrefs.GetInt("helpDelete",5); }
		set {PlayerPrefs.SetInt("helpDelete",value); }
	}
	
	int helpAdd {
		get {return PlayerPrefs.GetInt("helpAdd",8); }
		set {PlayerPrefs.SetInt("helpAdd",value); }
	}
	
	int skipsCount {
		get {return PlayerPrefs.GetInt("skipsCount",2); }
		set {PlayerPrefs.SetInt("skipsCount",value); }
	}
	
	
	float texturePositionX;
	float texturePositionY;
	float textureSizeX;
	float textureSizeY;
	float buttonSize;
	float buttonDistance;
	float answerYPosition;
	float buttonsYPosition;
	Rect texturePosition;
	int lettersCount;
	bool nextLevel;
	
	public GUISkin skin;
	public GUIStyle labelsStyle;
	public GUIStyle helpStyle;
	public GUIStyle invisible;
	
	bool[] canRemove;
	bool[] answeredButtons;
	string answeredString;
		
	private Dictionary <string, Texture> Letters = new Dictionary<string, Texture> ();
	
	private Process moveLetter;
	private Texture moveLetterTexture;
	private int moveLetterIndex;
	private Vector2 startPosition;
	private Vector2 targetPosition;
	private Vector2 currentPosition;
	
	private Texture rightAnswerTexture;
	private Texture nextLevelTexture;
	private Texture tryAgainTexture;
	private List<Texture> rightAnswerTextures = new List<Texture>();
		
	private int[] removes = new int[] {6,16,45,80};
	private int[] reveals = new int[] {6,16,35,80};
	private int[] skips = new int[] {1,4,16,25};
	
	private int pulseButtonIndex = 1;
	private Process pulse;
	private Process pulsation;
	private float deltaPulseTime = 5f;
	private float lastPulseTime = 0f;
	private float timeToNextPulse = 0f;

    #endregion
    //	Rect CoordinateCorrection(Rect rect) {}

    void HelpDelete() {
		if (helpDelete > 0){
			Debug.Log("Delete");
			helpDelete--;
			bool founded = false;
			for (int i = 0; i < parse.numberOfLetters; i++)
				if (!parse.lettersGen[i]) {
					parse.lettersGen[i] = true;
					answeredButtons[i] = false;
					Debug.Log(string.Format("Deleted {0} button", i));
					founded = true;
					break;			
				}
			if (!founded) {
				helpDelete++;	
			}
			return;
		}
		
		currentMenu = MenuType.BuyRemoves;
	}
	
	void HelpAdd() {
		if (helpAdd > 0){
			Debug.Log("Add");
			helpAdd--;
			for (int i = 0; i < parse.answerLength; i++)
				if (answeredString[i] == ' ') {
					currentAnswerLength++;
					char[] ch = answeredString.ToCharArray ();
					ch[i] = parse.answer[i];
					answeredString = new string(ch);
					moveLetter = new Process (0.15f);
					moveLetterIndex = currentAnswerLength;
					moveLetterTexture = GetLetterTexture(ch[i].ToString());
					targetPosition = PositionOfAnswerLetter(currentAnswerLength-1);
				
					for (int j = 0; i < parse.numberOfLetters; j++)
						if (parse.letters[j] == parse.answer[i] && parse.lettersGen[j] && answeredButtons[j]) {
							answeredButtons[j] = false;
							startPosition = PositionOfWriteLetter(j);
							break;
						}
					canRemove[i] = false;
					break;
				}
			return;
		}
		currentMenu =  MenuType.BuyReveals;
	}
	
	private string TwoLettersFromAnswer(string answer) {
	
		string result = "";
		int showIndex = Random.Range(0,answer.Length);
		int showIndex2 = Random.Range(0,answer.Length-1);
		if (showIndex2>showIndex) showIndex2++;
		for (int i = 0 ; i < answer.Length; i++) {
			if (i == showIndex || i == showIndex2) 
				result += answer[i];
			
			result += "_";
			
		}
		return result;
	}
	
	void LoadMission(bool next) {
		
		if (next) currentMission++;
		
		nextLevel = false;
		currentAnswerLength = 0;
		int lastLevel = currentLevel;
		if (next || parse == null) parse = new Parser(missions.text,ref currentMission,ref currentLevel);
		
		PlayerPrefs.SetInt("currentMision",currentMission);
		PlayerPrefs.SetInt("currentLevel",currentLevel);

		texture = Resources.Load("Textures/" + parse.image) as Texture2D;
//		Debug.LogWarning(parse.image);
		lettersCount = 0;
		for (int i = 0; i < parse.numberOfWords; i++)
			lettersCount += parse.correctAnswer[i].Length;
		answeredString = "";
		letterIndex = new int[lettersCount];
		for (int i = 0; i < lettersCount; i++){
			answeredString += " ";
			letterIndex[i] = -1;
		}
		answeredButtons = new bool[parse.numberOfLetters];
		for (int i = 0; i < answeredButtons.Length; i++)
			answeredButtons[i] = true;
		canRemove = new bool[parse.answerLength];
		for (int i = 0; i < parse.answerLength; i++)
			canRemove[i] = true;
		

	}
	
	public Texture GetLetterTexture(string letter) {
		return Letters[letter];
	}
	
	private void LoadLetters() {
		for (char i = 'A'; i!=(char)('Z' + 1); i = (char) (i+1)) {
			Letters.Add (i.ToString(),Resources.Load("Letters/" + i.ToString()) as Texture);
		}
		Letters.Add (" ",Resources.Load("Letters/" + "None") as Texture);
	}
	
	private void LoadTexts() {
		
		Object[] texts = Resources.LoadAll("Text");
		
		for (int i = 0; i < texts.Length; i++) {
			rightAnswerTextures.Add(texts[i] as Texture);
		}
		
	}
	
	void Start (){
        pulse = new Process(0);

        tryAgainTexture = Resources.Load("Images/Try_again") as Texture;
        nextLevelTexture = Resources.Load("Images/NextLevel") as Texture;

        moveLetter = new Process(0);

        LoadLetters();
        LoadTexts();

        labelsStyle.alignment = TextAnchor.MiddleCenter;
        labelsStyle.fontSize = 25;
        labelsStyle.fontStyle = FontStyle.Bold;

        helpStyle.alignment = TextAnchor.MiddleCenter;

        currentMission = PlayerPrefs.GetInt("currentMision", currentMission);
        currentLevel = PlayerPrefs.GetInt("currentLevel", currentLevel);

        currentMission = 1;

        //	helpStyle.fontSize = 20;
        helpStyle.font = Resources.Load("Fonts/TheUrbanWay/TheUrbanWay") as Font;
        Debug.Log(helpStyle.font);
        missions = Resources.Load("Missions") as TextAsset;
        LoadMission(false);

        buttonSize = Mathf.Min(Screen.width / 10, Screen.height / 15);
        buttonDistance = buttonSize / 4;

        textureSizeX = Screen.width / 3 * 2;
        textureSizeY = Screen.height * 0.4f;
        texturePositionX = Screen.width / 2 - Screen.width / 3;
        texturePositionY = buttonSize * 2;

        if (textureSizeX > textureSizeY)
        {
            texturePositionX += (textureSizeX - textureSizeY) / 2;
            textureSizeX = textureSizeY;
        }
        else
        {
            texturePositionY += (textureSizeY - textureSizeX) / 2;
            textureSizeY = textureSizeX;
        }


        buttonsYPosition = Screen.height - Screen.width / 8;
        answerYPosition = texturePositionY + textureSizeY * 1.1f;

        float deltaXY = textureSizeX / textureSizeY;
        float sizeX, sizeY;

        if (deltaXY > 1)
        {
            sizeX = Mathf.Min(textureSizeX, Screen.width * imageSizeX);
            sizeY = sizeX / deltaXY;
        }
        else
        {
            sizeY = Mathf.Min(textureSizeY, Screen.height * imageSizeY);
            sizeX = sizeY * deltaXY;
        }

        texturePosition = new Rect(Screen.width * imagePosX - sizeX / 2, Screen.height * imagePosY - sizeY / 2, sizeX, sizeY);
    }
	
	private Vector2 PositionOfAnswerLetter(int number) {
		
		int currentWord = 0;
		int currentLetter = 0;
		float currentXPos = Screen.width/2 - (parse.correctAnswer[currentWord].Length * buttonSize + parse.correctAnswer[currentWord].Length * buttonDistance - buttonDistance)/2;
		float answerYPosition = texturePositionY + textureSizeY*1.1f;
		
		
		for (int i = 0 ; i < number; i++) {
			currentXPos += buttonSize + buttonDistance;
			currentLetter++;
			if (currentLetter >= parse.correctAnswer[currentWord].Length) {
				currentLetter = 0;
				answerYPosition += buttonSize + buttonDistance;
				
				if (currentWord < parse.correctAnswer.Length - 1) 
					currentWord++;
				
				currentXPos = Screen.width/2 - (parse.correctAnswer[currentWord].Length * buttonSize + parse.correctAnswer[currentWord].Length * buttonDistance - buttonDistance)/2;
					
			}
		}
		
		return new Vector2 (currentXPos,answerYPosition  + moveUp);
	}
	
	private Vector2 PositionOfWriteLetter (int number) {
		
		int drawedButtonsCount = 0;
		float currentXPos = 0;
		float buttonsYPosition = Screen.height;
		
		for (int i = 0; i < number; i++)	{
			if (drawedButtonsCount % 7 == 0) {
				buttonsYPosition -= buttonSize + buttonDistance;
				currentXPos = Screen.width/2 - (7 * buttonSize + 6 * buttonDistance)/2;
				drawedButtonsCount++;
			}
			else drawedButtonsCount++;
			currentXPos += buttonSize + buttonDistance;
		}
		return new Vector2 (currentXPos, buttonsYPosition + moveUp);
	}
	
	private void Skip() {
		if (skipsCount>0) {
			LoadMission(true);	
			skipsCount--;
		}
	}
    
	private void GameMenu() {
      Debug.Log("GameMenu");
//		GUI.Box(new Rect(-1,-1,Screen.width+2,Screen.height+2),"",skin.GetStyle("Background"));
	
		if (Time.time > lastPulseTime + deltaPulseTime) {
			lastPulseTime = Time.time;
			pulseButtonIndex = 0;
			timeToNextPulse = 0.1f;
		}
		
		if (!pulse.Completed) {
			pulse.Update();
		}  else
		if (timeToNextPulse > 0) {
			timeToNextPulse--;	
			
			if (timeToNextPulse <=0) {
				pulseButtonIndex++;
				timeToNextPulse = 1.0f;
				
				if (pulseButtonIndex == 4) {
					pulseButtonIndex = 0;
					timeToNextPulse = 0f;
				}
				pulse = new Process (0.55f);
			}
		}
		
		if (Input.touchCount > 0 || Input.GetMouseButtonDown(0)) wrong = false;

        if (moveLetter.Completed)
            Debug.Log('c');

		if (moveLetter.Completed && nextLevel && currentMission > 10){
			GUI.Box(new Rect(Screen.width/4, Screen.height * 0.5f, Screen.width/2, 50), "WIN!!!");
			PlayerPrefs.SetInt("currentMision",1);
			
			return;
		}
		if (moveLetter.Completed && nextLevel) {
            //if (GUI.Button(new Rect(Screen.width/2 - 2*buttonSize, Screen.height * 0.7f, buttonSize*4, buttonSize),"",skin.customStyles[0] /* "Go to level " + (currentMission + 1).ToString ()*/)) LoadMission(true);

            NextLevelButton.SetActive(true);
            currentMenu = MenuType.NextLevel;
			LoadMission(true);
		}
		else {
			Debug.Log("else");
            /*
			if (GUI.Button(new Rect(Screen.width/2 - buttonSize/2f,Screen.height - buttonSize*1.5f, 
				buttonSize * (pulseButtonIndex == 2 && !pulse.Completed?(1.2f* (pulse.Position<0.5f?pulse.Position:1f-pulse.Position) + 1f*(1f-(pulse.Position<0.5f?pulse.Position:1f-pulse.Position)) ):1f),
				buttonSize * (pulseButtonIndex == 2 && !pulse.Completed?(1.2f* (pulse.Position<0.5f?pulse.Position:1f-pulse.Position) + 1f*(1f-(pulse.Position<0.5f?pulse.Position:1f-pulse.Position)) ):1f)),"", skin.GetStyle("Skip")))
				Skip();
                */
			GUI.Box(new Rect(Screen.width/2 + buttonSize/2f + 5f,Screen.height - buttonSize*1.5f, buttonSize, buttonSize), skipsCount.ToString(), helpStyle);
			
	//		GUI.DrawTexture(new Rect(Screen.width/2 - buttonSize * 4f,Screen.height - buttonSize*1.5f,buttonSize  * (pulseButtonIndex == 1 && !pulse.Completed?(1.2f* (pulse.Position<0.5f?pulse.Position:1f-pulse.Position) + 1f*(1f-(pulse.Position<0.5f?pulse.Position:1f-pulse.Position)) ):1f), buttonSize  * (pulseButtonIndex == 1 && !pulse.Completed?(1.2f* (pulse.Position<0.5f?pulse.Position:1f-pulse.Position) + 1f*(1f-(pulse.Position<0.5f?pulse.Position:1f-pulse.Position)) ):1f)), Resources.Load("buttons/remove") as Texture2D);
			GUI.Box(new Rect(Screen.width/2 - buttonSize * 3f + 5f,Screen.height - buttonSize*1.5f, buttonSize , buttonSize), helpDelete.ToString(), helpStyle);
			if (GUI.Button(new Rect(Screen.width/2 - buttonSize * 4.5f, Screen.height - buttonSize*1.5f, buttonSize * 2, buttonSize), "", invisible)) HelpDelete();
			
	//		GUI.DrawTexture(new Rect(Screen.width/2 + buttonSize * 2.5f, Screen.height - buttonSize*1.5f, buttonSize  * (pulseButtonIndex == 3 && !pulse.Completed?(1.2f* (pulse.Position<0.5f?pulse.Position:1f-pulse.Position) + 1f*(1f-(pulse.Position<0.5f?pulse.Position:1f-pulse.Position)) ):1f), buttonSize  * (pulseButtonIndex == 3 && !pulse.Completed?(1.2f* (pulse.Position<0.5f?pulse.Position:1f-pulse.Position) + 1f*(1f-(pulse.Position<0.5f?pulse.Position:1f-pulse.Position)) ):1f)), Resources.Load("buttons/reveal") as Texture2D);
			GUI.Box(new Rect(Screen.width/2 + buttonSize * 3.5f + 5, Screen.height - buttonSize*1.5f, buttonSize, buttonSize), helpAdd.ToString(), helpStyle);
			if (GUI.Button(new Rect(Screen.width/2 + buttonSize * 2.5f,Screen.height - buttonSize*1.5f, buttonSize * 2, buttonSize), "", invisible)) HelpAdd();
			
			int i;
			answerYPosition = texturePositionY + textureSizeY*1.1f;
			buttonsYPosition = Screen.height;
			
			int currentWord = 0;
			int currentLetter = 0;
			float currentXPos = Screen.width/2 - (parse.correctAnswer[currentWord].Length * buttonSize + parse.correctAnswer[currentWord].Length * buttonDistance - buttonDistance)/2;
			for (i = 0; i < answeredString.Length; i++){
				if (answeredString[i] == ' ' || !canRemove[i] || (!moveLetter.Completed && moveLetterIndex == i + 1)) { 
					
					if ((!moveLetter.Completed && moveLetterIndex == i + 1))
						GUI.Box(new Rect(currentXPos, answerYPosition + moveUp, buttonSize, buttonSize), GetLetterTexture(" "),skin.GetStyle("Answer"));
					else
						GUI.Box(new Rect(currentXPos, answerYPosition + moveUp, buttonSize, buttonSize), GetLetterTexture(answeredString[i].ToString()),skin.GetStyle("Answer"));
				}
				else if(moveLetter.Completed & GUI.Button(new Rect(currentXPos, answerYPosition + moveUp, buttonSize, buttonSize),GetLetterTexture(answeredString[i].ToString()), skin.GetStyle("Answer"))) {
		    		char[] ch = answeredString.ToCharArray();
					answeredButtons[letterIndex[i]] = true;
					
					
					moveLetterTexture = GetLetterTexture (ch[i].ToString());
					moveLetterIndex = - letterIndex[i] - 1;
					moveLetter = new Process (0.15f);
					startPosition = new Vector2 (currentXPos, answerYPosition + moveUp);
					targetPosition = PositionOfWriteLetter(letterIndex[i]);
					
					ch[i] = ' ';
					answeredString = new string (ch);
					currentAnswerLength--;
					
				}
				currentXPos += buttonSize + buttonDistance;
				currentLetter++;
				if (currentLetter >= parse.correctAnswer[currentWord].Length) {
//					Debug.Log("Current word: " + currentWord.ToString());
				//	Debug.Log("Length: " + parse.correctAnswer.Length.ToString());
					currentLetter = 0;
					answerYPosition += buttonSize + buttonDistance;
					if (currentWord < parse.correctAnswer.Length - 1)currentWord++;
					currentXPos = Screen.width/2 - (parse.correctAnswer[currentWord].Length * buttonSize + parse.correctAnswer[currentWord].Length * buttonDistance - buttonDistance)/2;
					
				}
			}
			
			int drawedButtonsCount = 0;
			
			for (i = 0; i < parse.numberOfLetters; i++)	{
				if (drawedButtonsCount % 7 == 0) {
					buttonsYPosition -= buttonSize + buttonDistance;
				//	if ((parse.numberOfLetters - drawedButtonsCount) < 5) currentXPos = Screen.width/2 - ((parse.numberOfLetters - drawedButtonsCount) * buttonSize + (parse.numberOfLetters - drawedButtonsCount) * buttonDistance - buttonDistance)/2;
				//	else 
					currentXPos = Screen.width/2 - (7 * buttonSize + 6 * buttonDistance)/2;
					drawedButtonsCount++;
				}
				else drawedButtonsCount++;
				if (answeredButtons[i] && !(!moveLetter.Completed && moveLetterIndex == -i - 1)) if (currentAnswerLength < answeredString.Length & moveLetter.Completed & GUI.Button(new Rect(currentXPos, buttonsYPosition + moveUp, buttonSize, buttonSize), GetLetterTexture( parse.letters[i].ToString()), skin.GetStyle("Box"))){
	
				/*	
					char[] ch;
					ch = answeredString.ToCharArray();
					ch[currentAnswerLength] = parse.letters[i];
					answeredString = new string(ch);
					letterIndex[currentAnswerLength] = i;
					answeredButtons[i] = false;
					currentAnswerLength++;
					wrong = false;
				*/


				char[] ch = answeredString.ToCharArray();
				for (int j = 0; j < answeredString.Length; j++)
					if (answeredString[j] == ' ') {
						ch[j] = parse.letters[i];
						letterIndex[j] = i;
						answeredButtons[i] = false;
						currentAnswerLength++;
						answeredString = new string(ch);
						
						moveLetterTexture = GetLetterTexture (ch[j].ToString());
						moveLetterIndex = j + 1;
						moveLetter = new Process (0.15f);
						startPosition = new Vector2 (currentXPos, buttonsYPosition  + moveUp);
						targetPosition = PositionOfAnswerLetter(j);
			
						break;
					}
				}
				else {}	
				else GUI.Box(new Rect(currentXPos, buttonsYPosition  + moveUp, buttonSize, buttonSize), "", skin.GetStyle("Box"));
			currentXPos += buttonSize + buttonDistance;
			}
			texture = Resources.Load("Textures/" + parse.image) as Texture2D;
			GUI.DrawTexture(texturePosition, texture);

            Debug.Log(answeredString);
            if (answeredString == parse.answer) {
				nextLevel = true;
			}
			
			if (wrong)GUI.Box(new Rect(Screen.width/4, Screen.height * 0.5f, Screen.width/2, 50), "Wrong answer");
			
			if (!moveLetter.Completed) {
				
				GUI.DrawTexture(new Rect(startPosition.x*(1f-moveLetter.Position) + targetPosition.x*(moveLetter.Position),
										 startPosition.y*(1f-moveLetter.Position) + targetPosition.y*(moveLetter.Position),
										 buttonSize,buttonSize),moveLetterTexture);
				
				moveLetter.Update();
			}
			
		}
	}	

    public void GUI_PlayButton() {
        StartButton.SetActive(false);
        currentMenu = MenuType.Game;
    }

    public void GUI_NextLevelButton() {
        NextLevelButton.SetActive(false);
        currentMenu = MenuType.Game;
    }
	
	void OnGUI() {
			
		if (currentMenu == MenuType.Game)
			GameMenu();
		
	}
	
}
