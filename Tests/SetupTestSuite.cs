/*
 * [File purpose]
 * Author: Phillip Piper
 * Date: 10/25/2008 10:31 PM
 * 
 * CHANGE LOG:
 * when who what
 * 10/25/2008 JPP  Initial Version
 */

using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightIdeasSoftware.Tests
{
	[TestClass]
	public class MyGlobals
	{
		[AssemblyInitialize]
		public static void RunBeforeAnyTests(TestContext context)
		{
			mainForm = new MainForm();
			mainForm.Size = new Size();
			mainForm.Show();
		}
		public static MainForm mainForm;

		[AssemblyCleanup]
		public static void RunAfterAnyTests()
		{
			mainForm?.Close();
		}
	}
}