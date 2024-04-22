﻿using System;

public struct AnimWaitStruct
{
	public string animName;
	public string nextState;
	public int layer;
	public Action startAction;
	public Action updateAction;
	public Action animStartAction;
	public Action exitAction;

	public AnimWaitStruct(string animName, string nextState = null, int layer = 0, Action startAction = null,
		Action updateAction = null, Action animStartAction = null, Action exitAction = null)
	{
		this.animName = animName;
		this.nextState = nextState;
		this.layer = layer;
		this.startAction = startAction;
		this.updateAction = updateAction;
		this.animStartAction = animStartAction;
		this.exitAction = exitAction;
	}
}