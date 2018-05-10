using System;
using System.Collections.Generic;
using System.IO;

namespace Data.PipelineSynchronous
{
	internal static class PipelineAdapter
	{
		public static Action<TIn, Action<TOut>> Scatter<TIn, TOut>(Func<TIn, List<TOut>> getAllItems)
		{
			return (source, handleData) =>
			{
				getAllItems(source)
					.ForEach(handleData);
			};
		}

		public static AdapterPipe<TIn, TOut> Scatter2<TIn, TOut>(Func<TIn, List<TOut>> getAllItems)
		{
			return new ScatterImpl<TIn, TOut>(getAllItems);
		}

		public static PipeSource<TIn, TOut> ScatterAsPipeSource<TIn, TOut>(Func<TIn, List<TOut>> getTheCards)
		{
			return new PipeSource<TIn, TOut>(Scatter(getTheCards));
		}

		internal class ScatterImpl<TIn, TOut> : AdapterPipe<TIn, TOut>
		{
			private readonly NodeDisplay _nodeDisplay;

			public ScatterImpl(Func<TIn, List<TOut>> handler)
			{
				_impl = input =>
				{
					List<TOut> result;
					try
					{
						result = handler(input);
					}
					catch (Exception err)
					{
						_Err(err);
						return;
					}
					foreach (var val in result)
					{
						_Notify(val);
					}
				};
				_nodeDisplay = new NodeDisplay($"Scatter({_Format(handler.Method)})");
			}

			public override void HandleData(TIn data)
			{
				_impl(data);
			}

			public override void HandleError(Exception error)
			{
				_Err(error);
			}

			public override void HandleDone()
			{
				_Finish();
			}

			public override void Describe(TextWriter output, int myLevel)
			{
				_nodeDisplay.WriteTo(output, myLevel);
				DescribeChildren(output, myLevel);
			}
		}
	}
}
