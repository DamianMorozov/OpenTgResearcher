﻿//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//namespace TgStorage.Domain.Sources;

//[DebuggerDisplay("{ToDebugString()}")]
//[DoNotNotify]
//public sealed class TgXpoSourceValidator : TgXpoTableValidatorBase<TgXpoSourceEntity>
//{
//	#region Public and private fields, properties, constructor

//	public TgXpoSourceValidator()
//	{
//		RuleFor(item => item.Id)
//				.GreaterThan(0);
//		RuleFor(item => item.UserName)
//				.NotNull();
//		RuleFor(item => item.Title)
//				.NotNull();
//		RuleFor(item => item.About)
//				.NotNull();
//		RuleFor(item => item.Count)
//				.GreaterThan(0);
//		//RuleFor(item => item.Directory)
//		//		.NotNull();
//		RuleFor(item => item.FirstId)
//			.GreaterThan(0);
//		//RuleFor(item => item.IsAutoUpdate)
//		//	.NotNull();
//	}

//	#endregion
//}