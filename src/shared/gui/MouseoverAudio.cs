using Godot;
using System;

public partial class MouseoverAudio : AudioStreamPlayer
{
	private AudioStreamPlayer ClickAudio => GetNode<AudioStreamPlayer>("%ClickAudio");
	
	public override void _Ready()
	{
		GetParent<Button>().MouseEntered += () =>
		{
			Play();
		};
		GetParent<Button>().Pressed += () =>
		{
			ClickAudio.Play();
		};
	}
}
