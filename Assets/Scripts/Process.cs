using UnityEngine;
using System.Collections;

public class Process {
	
	private bool looped;
	private float duration;
	private float currentTime;
	private float startTime;
	private float deltaTime = 0.001f;
	
	public float Position {
		get {return (currentTime - startTime) / duration;} 
		set {}
	}
	
	public bool Completed {
		get {return startTime + duration <= currentTime + deltaTime;}
		set {}
	}
	
	public Process (float duration) {
		looped = false;
		this.duration = duration;
		Restart();
	}
	
	public Process (float duration, bool isLooped) {
		looped = isLooped;
		this.duration = duration;
		Restart();
	}	
	
	// Update is called once per frame
	public void Update () {
		currentTime = Time.time;
		
		if (startTime + duration > currentTime) 
			return;
		
		if (looped) {
			Restart();
		} else {
			currentTime = startTime + duration;
		}
	}
	
	public void Restart () {
		startTime = Time.time;
		currentTime = Time.time;
	}
	
	public override string ToString ()
	{
		return string.Format ("[Process: Position={0}, Completed={1}, looped = {2}, StartTime = {3}, CurrentTime = {4}, duration = {5}]", 
			Position, Completed, looped, startTime, currentTime, duration);
	}
}
