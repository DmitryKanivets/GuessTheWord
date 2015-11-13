using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Parser {
	
	public bool[] lettersGen;
	int startElement = -1;
	public string[] correctAnswer;
	public int numberOfWords;
	public int numberOfLetters;
	public string letters;
	public int answerLength;
	public string answer;
	public string image;
	
	private string FindLevel(string missions, int level) {
		
		string result = "";
		bool found = false;
		StringReader reader = new StringReader (missions);
		
		while (!found) {
			string currentLine = reader.ReadLine();
			
			if (currentLine == null) 
				return "";
			
			if (currentLine[0] == '#') 
				continue;
			
			if (currentLine.Contains("Level:"+level)) {
				
				result +=currentLine +"\n";
				while (!currentLine.Contains("END_LEVEL")) {
					currentLine = reader.ReadLine();
					result += currentLine +"\n";
				}
			
				found = true;
			}
			
		}
		
		return result;
	}
	
	private string FindStage(string level, int stage) {
		string currentLine = "#";
		
		StringReader reader = new StringReader (level);
		while (currentLine != "") {
			currentLine = reader.ReadLine();
			
			if (currentLine == null) 
				return "";
				
			if (currentLine[0] == '#') 
				continue;
			
			if (currentLine.Contains("Stage:"+stage)) 
				return currentLine;
			
		}
		
		return "";
	}
	
	private string ReadCell(string target, string cell) {
		int position = target.IndexOf(cell) + cell.Length;
		//Debug.Log(cell + " " + target + " " +position);
		target = target.Substring(position, target.Length - position);
		//Debug.Log(target);
		return (target.Substring(0,target.IndexOf(";")));		
	}
	
	private string GetWord(ref string target) {
		if (target.IndexOf(' ') < 0) {
			string result1 = target;
			target = "";
			return result1;
		}
		string result = target.Substring(0,target.IndexOf(' '));
		target = target.Substring(target.IndexOf(' ') +1, target.Length - 1 - target.IndexOf(' '));
		return result;
		
	}
	
	public Parser(string missions,ref int missionNum,ref int levelNum) {
		
		string target = FindStage(FindLevel(missions,levelNum),missionNum);
		if (target == "") {
			levelNum++;
			missionNum = 1;
			target = FindStage(FindLevel(missions,levelNum),missionNum);
		}
		
		
		if (target == "") {
			levelNum = 1;
			missionNum = 1;
			target = FindStage(FindLevel(missions,levelNum),missionNum);
		}
		/*
		Debug.Log(ReadCell(target,"Image:"));
		Debug.Log(ReadCell(target,"Answer:"));
		Debug.Log(ReadCell(target,"NumberOfRows:"));
		*/
		string answerString = ReadCell(target,"Answer:");
		numberOfWords = 0;
		List <string> answerWords = new List<string> ();
		while (answerString != "") {
			answerWords.Add(GetWord(ref answerString));	
			numberOfWords++;
		}
		
		int j = 0;
		image = ReadCell(target,"Image:");
		correctAnswer = new string[numberOfWords];
		for (int i = 0; i < numberOfWords; i++){
			correctAnswer[i] = answerWords[i];
		}

        string randomChar = "QWERTYUIOPASDFGHJKLZXCVBNM";

        numberOfLetters = 7 * int.Parse(ReadCell(target,"NumberOfRows:"));
		lettersGen = new bool[numberOfLetters];
		for (int i = 0; i < numberOfLetters; i++)
			letters += randomChar[Random.Range(0, randomChar.Length)];
		
		
		for (int i = 0; i < numberOfLetters; i++)
			lettersGen[i] = false;
		int x =0;
		answerLength = 0;
		answer = "";
		for (j = 0; j < correctAnswer.Length; j++) {
			answerLength += correctAnswer[j].Length;
			answer += correctAnswer[j];
			for (int i = 0; i < correctAnswer[j].Length; x++) {
				int c = Random.Range(0, numberOfLetters);
				if (!lettersGen[c]) {
					char[] ch = letters.ToCharArray();
					ch[c] = correctAnswer[j][i];
					letters = new string(ch);
					lettersGen[c] = true;
					i++;
				}
			}
		}
		Debug.Log(answer);
	}
	
}
