using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldloc)]
	public class Ldloc : ILOp
	{
		public Ldloc(Cosmos.Compiler.Assembler.Assembler aAsmblr)
			: base(aAsmblr)
		{
		}

		public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
		{
			var xOpVar = (OpVar)aOpCode;
			var xVar = aMethod.MethodBase.GetMethodBody().LocalVariables[xOpVar.Value];
			var xStackCount = GetStackCountForLocal(aMethod, xVar);
			var xEBPOffset = ((int)GetEBPOffsetForLocal(aMethod, xOpVar.Value));
			var xSize = SizeOfType(xVar.LocalType);
            new Comment("EBPOffset = " + xEBPOffset); 
            if (xStackCount > 1)
			{
				for (int i = 0; i < xStackCount; i++)
				{
					new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = (int)(0 - (xEBPOffset + (i * 4))) };
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
				}
			}
			else
			{
				switch (xSize)
				{
					case 1:
					case 2:
						{
							bool signed = IsIntegerSigned(xVar.LocalType);
							if (signed)
								new CPUx86.MoveSignExtend { DestinationReg = CPUx86.Registers.EAX, Size = (byte)(xSize * 8), SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0 - xEBPOffset };
							else
								new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.Registers.EAX, Size = (byte)(xSize * 8), SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0 - xEBPOffset };
							new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
							break;
						}
					case 4:
						{
							new CPUx86.Push { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0 - xEBPOffset };
							break;
						}
				}
			}
			Assembler.Stack.Push(ILOp.Align(xSize, 4), xVar.LocalType);
		}
	}
}