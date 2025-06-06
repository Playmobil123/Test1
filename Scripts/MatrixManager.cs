using System.Collections.Generic;
using System.Numerics; // Or replace with your own Matrix4x4 type

public static class MatrixManager {
    public static int CurrentFrame = 0;

    private static List<Matrix4x4> matrixStack = new List<Matrix4x4>();

    public static Matrix4x4 CurrentMatrix => matrixStack.Count > 0 ? matrixStack[matrixStack.Count - 1] : Matrix4x4.Identity;

    public static void InitGlobalFrameCounter(uint frame) {
        CurrentFrame = (frame != 0xFFFFFFFF) ? (int)frame : 0;
    }

    public static void InitMatrixStack() {
        matrixStack.Clear();
        matrixStack.Add(Matrix4x4.Identity);
    }

    public static void PushMatrix(Matrix4x4 matrix) {
        matrixStack.Add(Matrix4x4.Multiply(CurrentMatrix, matrix));
    }

    public static void PopMatrix() {
        if (matrixStack.Count > 1) {
            matrixStack.RemoveAt(matrixStack.Count - 1);
        }
    }

    public static void LoadMatrix(Matrix4x4 matrix) {
        matrixStack.Add(matrix);
    }

    public static void ResetStack() {
        matrixStack.Clear();
    }
}
