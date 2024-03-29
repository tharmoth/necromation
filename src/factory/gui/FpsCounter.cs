using Godot;
using System;
using Necromation;

public partial class FpsCounter : Label
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var portOfCall = Globals.Player.GetViewport();
		var processTime = Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000;
		var physicsProcessTime = Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess) * 1000;
		RenderingServer.ViewportSetMeasureRenderTime(portOfCall.GetViewportRid(), true);
		var renderTimeCpu = RenderingServer.ViewportGetMeasuredRenderTimeCpu(portOfCall.GetViewportRid());
		var renderTimeGpu = RenderingServer.ViewportGetMeasuredRenderTimeGpu(portOfCall.GetViewportRid());

		processTime = Math.Round(processTime, 2);
		physicsProcessTime = Math.Round(physicsProcessTime, 2);
		
		Text = Engine.GetFramesPerSecond() + " FPS" + 
			$"\nProcess: {processTime}ms" + 
			$"\nPhysics: {physicsProcessTime}ms" + 
			$"\nRender CPU: {renderTimeCpu}ms" + 
			$"\nRender GPU: {renderTimeGpu}ms";
	}
}
