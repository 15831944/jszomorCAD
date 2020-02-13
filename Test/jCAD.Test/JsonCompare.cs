﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using jCAD.PID_Builder;
using JsonParse;
using JsonFindKey;

namespace jCAD.Test
{
	[TestClass]
	public class JsonCompare
	{
		[TestMethod]
		public void JsonTest()
		{
			string jsonInput = @"E:\Jszomor\source\repos\jszomorCAD\jCAD.PID_Builder\JsonPIDBuild.json";
			var input = System.IO.File.ReadAllLines(jsonInput);

			//foreach (var item in input)
			//{
			//  Console.WriteLine(item);
			//}

			string jsonOutput = @"E:\Jszomor\source\repos\jszomorCAD\jCAD.PID_Builder\JsonPIDBuildCopy.json";
			var output = System.IO.File.ReadAllLines(jsonOutput);

			if (output.Length == input.Length)
			{
				//Assert.IsTrue(true);
				for (int i = 0; i < input.Length; i++)
				{
					Assert.AreEqual(input[i], output[i]);
				}
			}
			else
			{
				Assert.IsTrue(false);
			}
		}

		[TestMethod]
		public void HashCompare()
		{
			string jsonInput = @"E:\Jszomor\source\repos\jszomorCAD\jCAD.PID_Builder\JsonPIDBuild.json";
			string jsonOutput = @"E:\Jszomor\source\repos\jszomorCAD\jCAD.PID_Builder\JsonPIDBuildCopy.json";

			var inputFileInfo = new System.IO.FileInfo(jsonInput);
			var outputFileInfo = new System.IO.FileInfo(jsonOutput);
			if (inputFileInfo.Length == outputFileInfo.Length && inputFileInfo.Length > 0)
			{
				byte[] hash1, hash2;
				using (HashAlgorithm ha = HashAlgorithm.Create())
				{
					using (FileStream f1 = new FileStream(jsonInput, FileMode.Open))
					{
						using (FileStream f2 = new FileStream(jsonOutput, FileMode.Open))
						{
							/* Calculate Hash */
							hash1 = ha.ComputeHash(f1);
							hash2 = ha.ComputeHash(f2);
						}
					}
				}
				/* Show Hash in TextBoxes */
				var jsonInputHash = BitConverter.ToString(hash1);
				var jsonOutputHash = BitConverter.ToString(hash2);
				/* Compare the hash and Show Message box */
				Assert.AreEqual(jsonInputHash, jsonOutputHash);
				if (jsonInputHash == jsonOutputHash)
				{
					MessageBox.Show("Files are Equal !");
				}
				else
				{
					MessageBox.Show("Files are Different !");
				}
			}
			else
			{
				Assert.IsTrue(false);

				MessageBox.Show("Files have different length!");
			}
		}

		[TestMethod]
		public void FilesAreEqual_Hash()
		{
			FileInfo first = new FileInfo(@"E:\Jszomor\source\repos\jszomorCAD\jCAD.PID_Builder\JsonPIDBuild.json");
			FileInfo second = new FileInfo(@"E:\Jszomor\source\repos\jszomorCAD\jCAD.PID_Builder\JsonPIDBuildCopy.json");
			byte[] firstHash = MD5.Create().ComputeHash(first.OpenRead());
			byte[] secondHash = MD5.Create().ComputeHash(second.OpenRead());

			for (int i = 0; i < firstHash.Length; i++)
			{
				if (firstHash[i] != secondHash[i])
					Assert.IsTrue(false);
			}
			Assert.IsTrue(true);
		}

		public static void CommentCollector(List<object> comments, string path)
		{
			using (StreamWriter sw = WriteToFile(path))
			{
				foreach (var comment in comments)
				{
					sw.WriteLine(Convert.ToString(comment), path);
				}

				sw.Close();
			}
		}
		public static StreamWriter WriteToFile(string path)
		{
			//if (Directory.Exists(path) == false)
			//{
			//	Directory.CreateDirectory(path);
			//}
			FileInfo fi = new FileInfo(path);
			StreamWriter streamWriteOutput = fi.CreateText();
			return streamWriteOutput;			
		}

		[TestMethod]
		public void JsonCompareResult()
		{
			//Assert.IsTrue(DeepComparer());
			DeepCompare();
		}

		public bool DeepCompare()
		{
			bool isIdentical = false;
			var deepex = new DeepEx();
			string FilePath = @"E:\Jszomor\source\repos\jszomorCAD\jCAD.PID_Builder\JsonCompareResult.txt";
			var fileName1 = @"E:\Jszomor\source\repos\jszomorCAD\jCAD.PID_Builder\JsonPIDBuild.json";
			//var fileName1 = @"C:\Users\JANO\source\repos\jszomorCAD\jCAD.PID_Builder\JsonPIDBuild.json"; //DELL
			var fileName2 = @"E:\Jszomor\source\repos\jszomorCAD\jCAD.PID_Builder\JsonPIDBuildCopy.json";
			//var fileName2 = @"C:\Users\JANO\source\repos\jszomorCAD\jCAD.PID_Builder\JsonPIDBuildCopy.json"; //DELL
			var blockDeserialize = new BlockDeserializer();
			var jsonPID1 = blockDeserialize.ReadJsonData(fileName1);
			var jsonPID2 = blockDeserialize.ReadJsonData(fileName2);		
			
			BlockCompare(jsonPID1, jsonPID2, deepex);
			LineCompare(jsonPID1, jsonPID2, deepex);
			
			if (jsonPID1.Blocks.Count != jsonPID2.Blocks.Count || jsonPID1.Lines.Count != jsonPID2.Lines.Count)
			{
				deepex.Comments.Insert(0, "Differences:");
				deepex.Comments.Add("Length is not equal!");
			}

			if (deepex.Comments.Count == 0)
			{
				deepex.Comments.Add("Files are eqvivalent!");
				isIdentical = true;
			}
			else 
			{
				if (deepex.Comments.Contains("Differences:") == false)
					deepex.Comments.Insert(0, "Differences:");

				deepex.Comments.Add("Files are not eqvivalent!");
			}
			CommentCollector(deepex.Comments, FilePath);
			return isIdentical;
		}
		public void BlockCompare(JsonPID jsonPID1, JsonPID jsonPID2, DeepEx deepEx)
		{
			var dictBlock1 = new Dictionary<int, JsonBlockProperty>();
			var dictBlock2 = new Dictionary<int, JsonBlockProperty>();

			for (int i = 0; i < jsonPID1.Blocks.Count; i++)
			{
				dictBlock1.Add(jsonPID1.Blocks[i].Attributes.Internal_Id, jsonPID1.Blocks[i]);
				dictBlock2.Add(jsonPID2.Blocks[i].Attributes.Internal_Id, jsonPID2.Blocks[i]);
			}
			foreach (var i in dictBlock1)
			{
				if (dictBlock2.TryGetValue(i.Key, out JsonBlockProperty compareValue))
				{
					deepEx.BlockGeometryCompare(i.Value, compareValue);
					deepEx.BlockMiscCompare(i.Value, compareValue);
					deepEx.BlockGeneralCompare(i.Value, compareValue);
					deepEx.BlockCustomCompare(i.Value, compareValue);
					deepEx.BlockAttributesCompare(i.Value, compareValue);
				}
			}
		}
		public void LineCompare(JsonPID jsonPID1, JsonPID jsonPID2, DeepEx deepEx)
		{
			var dictLines1 = new Dictionary<int, JsonLineProperty>();
			var dictLines2 = new Dictionary<int, JsonLineProperty>();

			for (int i = 0; i < jsonPID1.Lines.Count; i++)
			{
				dictLines1.Add(jsonPID1.Lines[i].Internal_Id, jsonPID1.Lines[i]);
				dictLines2.Add(jsonPID2.Lines[i].Internal_Id, jsonPID2.Lines[i]);
			}
			foreach (var i in dictLines1)
			{
				if (dictLines2.TryGetValue(i.Key, out JsonLineProperty compareValue))
				{
					deepEx.LineTypeComparer(i.Value, compareValue);
					deepEx.LinePointsComparer(i.Value, compareValue);
				}
			}
		}
	}
}
