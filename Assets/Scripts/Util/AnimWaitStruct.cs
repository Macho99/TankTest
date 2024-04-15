using System;

public struct AnimWaitStruct
{
	public string animName;
	public string nextState;
	public Action startAction;
	public Action updateAction;
	public Action animStartAction;
	public Action animEndAction;

	public AnimWaitStruct(string animName, string nextState = null, Action startAction = null,
		Action updateAction = null, Action animStartAction = null, Action animEndAction = null)
	{
		this.animName = animName;
		this.nextState = nextState;
		this.startAction = startAction;
		this.updateAction = updateAction;
		this.animStartAction = animStartAction;
		this.animEndAction = animEndAction;
	}
}