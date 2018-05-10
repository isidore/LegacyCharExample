﻿using System;

namespace Data.Tests.ExecutionPipelineWorks
{
	public class PipeSource<TIn, TOut>
	{
		public delegate void Notify(PossibleResult<TOut> result);

		private readonly Action<TIn> _impl;
		private string _text;

		public PipeSource(Func<TIn, TOut> handler)
		{
			_impl = input =>
			{
				TOut result;
				try
				{
					result = handler(input);
				}
				catch (Exception err)
				{
					_Err(err);
					return;
				}
				_Notify(result);
				_Finish();
			};
			_text = handler.ToString();
		}

		public PipeSource(Action<TIn, Action<TOut>> handler)
		{
			_impl = input =>
			{
				try
				{
					handler(input, _Notify);
				}
				catch (Exception err)
				{
					_Err(err);
					return;
				}
				_Finish();
			};
			_text = handler.ToString();
		}

		private void _Err(Exception error)
		{
			ResultGenerated?.Invoke(PossibleResult<TOut>.Of(error));
		}

		private void _Notify(TOut result)
		{
			ResultGenerated?.Invoke(PossibleResult<TOut>.Of(result));
		}

		private void _Finish()
		{
			ResultGenerated?.Invoke(PossibleResult<TOut>.Done());
		}

		public event Notify ResultGenerated;

		public void Call(TIn input)
		{
			_impl(input);
		}

		public void AddSegments(IHandleResult<TOut> downstream)
		{
			ResultGenerated += evt => evt.Handle(downstream);
		}
	}
}
