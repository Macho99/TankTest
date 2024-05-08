using System;

public struct AnimWaitStruct
{
	public string animName;
	public string nextState;
	public int layer;
	public string startAnimName;
	public Action startAction;
	public Action updateAction;
	public Action animStartAction;
	public Action exitAction;
	public Action nextStateAction;

	public AnimWaitStruct(string animName, string nextState = null, int layer = 0, 
		string startAnimName = null, Action startAction = null,
		Action updateAction = null, Action animStartAction = null, Action exitAction = null, 
		Action nextStateAction = null)
	{
		this.animName = animName;
		this.nextState = nextState;
		this.layer = layer;
		this.startAnimName = startAnimName == null ? animName : startAnimName;
		this.startAction = startAction;
		this.updateAction = updateAction;
		this.animStartAction = animStartAction;
		this.exitAction = exitAction;
		this.nextStateAction = nextStateAction;
	}
}