﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AiSoft.Nat.Utils
{
	internal static class StreamExtensions
	{
		internal static string ReadAsMany(this StreamReader stream, int bytesToRead)
		{
			var buffer = new char[bytesToRead];
			stream.ReadBlock(buffer, 0, bytesToRead);
			return new string(buffer);
		}

		internal static string GetXmlElementText(this XmlNode node, string elementName)
		{
			var element = node[elementName];
			return element != null ? element.InnerText : string.Empty;
		}

		internal static bool ContainsIgnoreCase(this string s, string pattern)
		{
			return s.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		internal static void LogInfo(this TraceSource source, string format, params object[] args)
		{
			try
			{
				source.TraceEvent(TraceEventType.Information, 0, format, args);
			}
			catch (ObjectDisposedException)
			{
				source.Switch.Level = SourceLevels.Off;
			}
		}

		internal static void LogWarn(this TraceSource source, string format, params object[] args)
		{
			try
			{
				source.TraceEvent(TraceEventType.Warning, 0, format, args);
			}
			catch (ObjectDisposedException)
			{
				source.Switch.Level = SourceLevels.Off;
			}
		}

		internal static void LogError(this TraceSource source, string format, params object[] args)
		{
			try
			{
				source.TraceEvent(TraceEventType.Error, 0, format, args);
			}
			catch (ObjectDisposedException)
			{
				source.Switch.Level = SourceLevels.Off;
			}
		}

		internal static string ToPrintableXml(this XmlDocument document)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new XmlTextWriter(stream, Encoding.Unicode))
				{
					try
					{
						writer.Formatting = Formatting.Indented;

						document.WriteContentTo(writer);
						writer.Flush();
						stream.Flush();
                        // Have to rewind the MemoryStream in order to read
						// its contents.
						stream.Position = 0;
                        // Read MemoryStream contents into a StreamReader.
						var reader = new StreamReader(stream);
                        // Extract the text from the StreamReader.
						return reader.ReadToEnd();
					}
					catch (Exception)
					{
						return document.ToString();
					}
				}
			}
		}

		public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
		{
            var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
			if (completedTask == task)
			{
				timeoutCancellationTokenSource.Cancel();
				return await task;
			}
			throw new TimeoutException("The operation has timed out. The network is broken, router has gone or is too busy.");
		}
    }
}