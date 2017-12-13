// the names of the cockpit and LCD respectively
public const string alignment_cockpit = "alignment cockpit";
public const string alignment_lcd = "alignment lcd";


// the character that separates the 2 GPS coordinates
public const char GPS_delimiter = '&';





public Program() {
	Runtime.UpdateFrequency = UpdateFrequency.Once;
	setup();
}

public void Main() {

	// testing blocks
	if(controller == null || !controller.IsWorking) {
		setup();
		Echo("controller is not working");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}
	if(textpanel == null || !textpanel.IsWorking) {
		setup();
		Echo("text panel is not working");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}




	// starting string manipulation
	string[] gpsText = Me.CustomData.Split(GPS_delimiter);

	if(gpsText.Count() != 2) {
		Echo("GPS coordinates are wrong\nneed 2 GPS coordinates seperated by a '&'");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}





	string[] gps0 = gpsText[0].Split(':');

	if(gps0.Count() != 6) {
		Echo("first GPS coordinate is wrong");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}
	string[] gps1 = gpsText[1].Split(':');

	if(gps1.Count() != 6) {
		Echo("second GPS coordinate is wrong");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}




	// converting strings to numbers
	double gpsaX;
	if(!Double.TryParse(gps0[2], out gpsaX)) {
		Echo("first GPS coordinate is wrong (X)");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}
	double gpsaY;
	if(!Double.TryParse(gps0[3], out gpsaY)) {
		Echo("first GPS coordinate is wrong (Y)");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}
	double gpsaZ;
	if(!Double.TryParse(gps0[4], out gpsaZ)) {
		Echo("first GPS coordinate is wrong (Z)");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}

	double gpsbX;
	if(!Double.TryParse(gps1[2], out gpsbX)) {
		Echo("second GPS coordinate is wrong (X)");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}
	double gpsbY;
	if(!Double.TryParse(gps1[3], out gpsbY)) {
		Echo("second GPS coordinate is wrong (Y)");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}
	double gpsbZ;
	if(!Double.TryParse(gps1[4], out gpsbZ)) {
		Echo("second GPS coordinate is wrong (Z)");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}


	// putting the numbers into vectors
	Vector3D gpsa = new Vector3D(gpsaX, gpsaY, gpsaZ);
	Vector3D gpsb = new Vector3D(gpsbX, gpsbY, gpsbZ);


	// final test
	if(gpsa == gpsb) {
		Echo("coordinates are the same");
		Runtime.UpdateFrequency = UpdateFrequency.Update100;
		return;
	}

	Echo("All tests passed");
	Runtime.UpdateFrequency = UpdateFrequency.Update1;




	// the math
	Vector3D line = gpsb - gpsa;
	line = Vector3D.Normalize(line);

	double dot = controller.WorldMatrix.Forward.dot(line);
	Vector3D diff = line - controller.WorldMatrix.Forward;
	diff = diff.TransformNormal(controller.WorldMatrix.Invert());



	// writing results
	textpanel.WritePublicText($"aligned: {dot}\n", false);
	textpanel.WritePublicText($"diff X: {diff.X}\n", true);
	textpanel.WritePublicText($"diff Y: {diff.Y}\n", true);
	textpanel.WritePublicText($"diff Z: {diff.Z}\n", true);



}

public IMyShipController controller;
public IMyTextPanel textpanel;

public void setup() {
	controller = (IMyShipController)GridTerminalSystem.GetBlockWithName(alignment_cockpit);
	textpanel = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(alignment_lcd);
}


// not efficient, just a simple debugging tool
// public const bool debug = true;
// public void log(string text, bool append = true) {
// 	if(!debug) return;
// 	List<IMyTextPanel> panels = new List<IMyTextPanel>();

// 	GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(panels, block => block is IMyTextPanel && block.CustomName.ToLower().Contains("debug"));

// 	if(append) {
// 		text += "\n";
// 	}

// 	foreach(IMyTextPanel panel in panels) {
// 		panel.WritePublicText(text, append);
// 	}
// }



}
public static class CustomProgramExtensions {

	public static double dot(this Vector3D a, Vector3D b) {
		return Vector3D.Dot(a, b);
	}

	public static Vector3D project(this Vector3D a, Vector3D b) {
		double adb = a.dot(b);
		double bdb = b.dot(b);
		return b * adb / bdb;
	}

	public static Vector3D reject(this Vector3D a, Vector3D b) {
		return Vector3D.Reject(a, b);
	}

	public static void setThrust(this IMyThrust thruster, Vector3D desired) {
		var proj = desired.project(thruster.WorldMatrix.Backward);

		if(proj.dot(thruster.WorldMatrix.Backward) > 0) {//negative * negative is positive... so if its greater than 0, you ignore it.
			thruster.ThrustOverride = 0;
			return;
		}

		thruster.ThrustOverride = (float)proj.Length();
	}

	public static void setThrust(this IMyThrust thruster, Vector3D desired, out string error) {
		error = "";
		var proj = desired.project(thruster.WorldMatrix.Backward);

		if(proj.dot(thruster.WorldMatrix.Backward) > 0) {//negative * negative is positive... so if its greater than 0, you ignore it.
			thruster.ThrustOverride = 0;
			error += "wrong way";
			return;
		}
		error += $"right way";
		error += $"\nproportion: {(proj.Length() / desired.Length()).Round(2)}";
		error += $"\nproj: {proj.Length().Round(1)}";
		error += $"\ndes: {desired.Length().Round(1)}";

		error += $"\nproj: {proj.Round(1)}";
		error += $"\ndesired: {desired.Round(1)}";

		thruster.ThrustOverride = (float)proj.Length();
	}


	public static Vector3D Round(this Vector3D vec, int num) {
		return Vector3D.Round(vec, num);
	}

	public static double Round(this double val, int num) {
		return Math.Round(val, num);
	}

	public static float Round(this float val, int num) {
		return (float)Math.Round(val, num);
	}

	public static Vector3D getWorldMoveIndicator(this IMyShipController controller) {
		return Vector3D.TransformNormal(controller.MoveIndicator, controller.WorldMatrix);
	}

	public static bool IsNaN(this double val) {
		return double.IsNaN(val);
	}

	public static Vector3D NaNtoZero(this Vector3D val) {
		if(val.X.IsNaN()) {
			val.X = 0;
		}
		if(val.Y.IsNaN()) {
			val.Y = 0;
		}
		if(val.Z.IsNaN()) {
			val.Z = 0;
		}

		return val;
	}

	public static Vector3 GetVector(this Base6Directions.Direction dir) {
		return Base6Directions.GetVector(dir);
	}

	public static Vector3D TransformNormal(this Vector3D vec, MatrixD mat) {
		return Vector3D.TransformNormal(vec, mat);
	}

	public static MatrixD Invert(this MatrixD mat) {
		return MatrixD.Invert(mat);
	}

	public static double ToDouble(this string str) {
		double ret;
		if(!Double.TryParse(str, out ret)) {
			return 0;
		}
		return ret;
	}