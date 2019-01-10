using System;

namespace MDLN.Tools {
	/// <summary>
	/// A vector represented in polar notation
	/// </summary>
	/// <typeparam name="T">Data type to store the component values in, must be numeric</typeparam>
	public struct VectorPolar<T> {
		/// <summary>
		/// Vector's length
		/// </summary>
		public T Length;
		/// <summary>
		/// Vector's Angle
		/// </summary>
		public T Angle;
	}

	/// <summary>
	/// A vector represented in rectangular notation
	/// </summary>
	/// <typeparam name="T">Data type to store the component values in, must be numeric</typeparam>
	public struct VectorRect<T> {
		/// <summary>
		/// Vector's Real or X component
		/// </summary>
		public T Real;
		/// <summary>
		/// Vector's Imaginary or Y component
		/// </summary>
		public T Imaginary;
	}

	/// <summary>
	/// Class that represents a vector and allows operations on that vector
	/// </summary>
	public class Vector {
		private VectorPolar<double> cPolar;
		private VectorRect<double> cRect;

		/// <summary>
		/// Gets the coordinates of the vector in polar notation using values of type int
		/// </summary>
		public VectorPolar<int> PolarInt {
			get {
				VectorPolar<int> VPolar = new VectorPolar<int>();

				VPolar.Length = (int)cPolar.Length;
				VPolar.Angle = (int)cPolar.Angle;

				return VPolar;
			}
		}

		/// <summary>
		/// Gets the coordinates of the vector in rectangular notation using values of type int
		/// </summary>
		public VectorRect<int> RectangularInt {
			get {
				VectorRect<int> VRect = new VectorRect<int>();

				VRect.Real = (int)cRect.Real;
				VRect.Imaginary = (int)cRect.Imaginary;

				return VRect;
			}
		}

		/// <summary>
		/// Gets the coordinates of the vector in polar notation using values of type double
		/// </summary>
		public VectorPolar<double> Polar {
			get {
				return cPolar;
			}
		}

		/// <summary>
		/// Gets the coordinates of the vector in rectangular notation using values of type double
		/// </summary>
		public VectorRect<double> Rectangular {
			get {
				return cRect;
			}
		}

		/// <summary>
		/// Sets the value of the vector using a polar notation
		/// </summary>
		/// <param name="Length">Length to make the vector</param>
		/// <param name="Angle">Angle to give the vector</param>
		/// <returns>The vector object after being modified</returns>
		public Vector SetPolarCoordinates(int Length, int Angle) {
			cPolar.Length = Length;
			cPolar.Angle = Angle;

			ConvertPolarToRect();

			return this;
		}

		/// <summary>
		/// Sets the value of the vector using a polar notation
		/// </summary>
		/// <param name="Length">Length to make the vector</param>
		/// <param name="Angle">Angle to give the vector</param>
		/// <returns>The vector object after being modified</returns>
		public Vector SetPolarCoordinates(double Length, double Angle) {
			cPolar.Length = Length;
			cPolar.Angle = Angle;

			ConvertPolarToRect(Length, Angle);

			return this;
		}

		/// <summary>
		/// Sets the value of the vector using a rectangular notation
		/// </summary>
		/// <param name="Real">Real component to give the vector</param>
		/// <param name="Imaginary">Imaginary component to give the vector</param>
		/// <returns>The vector object after being modified</returns>
		public Vector SetRectangularCoordinates(int Real, int Imaginary) {
			cRect.Real = Real;
			cRect.Imaginary = Imaginary;

			ConvertRectToPolar();

			return this;
		}

		/// <summary>
		/// Sets the value of the vector using a rectangular notation
		/// </summary>
		/// <param name="Real">Real component to give the vector</param>
		/// <param name="Imaginary">Imaginary component to give the vector</param>
		/// <returns>The vector object after being modified</returns>
		public Vector SetRectangularCoordinates(double Real, double Imaginary) {
			cRect.Real = Real;
			cRect.Imaginary = Imaginary;

			ConvertRectToPolar(Real, Imaginary);

			return this;
		}

		/// <summary>
		/// Add a vector in polar notation to this vector
		/// </summary>
		/// <param name="Length">Length of the vector to add</param>
		/// <param name="Angle">Angle of the vector to add</param>
		/// <returns>This vector after it has been updated</returns>
		public Vector AddPolarVector(double Length, double Angle) {
			double NewReal = cRect.Real;
			double NewImaginary = cRect.Imaginary;

			SetPolarCoordinates(Length, Angle);

			NewReal += cRect.Real;
			NewImaginary += cRect.Imaginary;

			SetRectangularCoordinates(NewReal, NewImaginary);

			return this;
		}

		/// <summary>
		/// Add a vector in rectangular notation to this vector
		/// </summary>
		/// <param name="Real">Real component of the vector to add</param>
		/// <param name="Imaginary">Imaginary component of the vector to add</param>
		/// <returns>This vector after it has been updated</returns>
		public Vector AddRectangularVector(double Real, double Imaginary) {
			cRect.Real += Real;
			cRect.Imaginary += Imaginary;

			ConvertRectToPolar();

			return this;
		}

		/// <summary>
		/// Add a vector to this vector
		/// </summary>
		/// <param name="NewVector">Vector to add</param>
		/// <returns>This vector after it has been updated</returns>
		public Vector AddVector(Vector NewVector) {
			cRect.Real += NewVector.Rectangular.Real;
			cRect.Imaginary += NewVector.Rectangular.Imaginary;

			ConvertRectToPolar();

			return this;
		}

		/// <summary>
		/// Subtract a vector in polar notation to this vector
		/// </summary>
		/// <param name="Length">Length of the vector to subtract</param>
		/// <param name="Angle">Angle of the vector to subtract</param>
		/// <returns>This vector after it has been updated</returns>
		public Vector SubtractPolarVector(double Length, double Angle) {
			double NewReal = cRect.Real;
			double NewImaginary = cRect.Imaginary;

			SetPolarCoordinates(Length, Angle);

			NewReal -= cRect.Real;
			NewImaginary -= cRect.Imaginary;

			SetRectangularCoordinates(NewReal, NewImaginary);

			return this;
		}

		/// <summary>
		/// Subtract a vector in rectangular notation to this vector
		/// </summary>
		/// <param name="Real">Real component of the vector to subtract</param>
		/// <param name="Imaginary">Imaginary component of the vector to subtract</param>
		/// <returns>This vector after it has been updated</returns>
		public Vector SubtractRectangularVector(double Real, double Imaginary) {
			cRect.Real -= Real;
			cRect.Imaginary -= Imaginary;

			ConvertRectToPolar();

			return this;
		}

		/// <summary>
		/// Subtract a vector to this vector
		/// </summary>
		/// <param name="NewVector">Vector to subtract</param>
		/// <returns>This vector after it has been updated</returns>
		public Vector SubtractVector(Vector NewVector) {
			cRect.Real -= NewVector.Rectangular.Real;
			cRect.Imaginary -= NewVector.Rectangular.Imaginary;

			ConvertRectToPolar();

			return this;
		}

		/// <summary>
		/// Multiply a vector in polar notation to this vector
		/// </summary>
		/// <param name="Length">Length of the vector to multiply</param>
		/// <param name="Angle">Angle of the vector to multiply</param>
		/// <returns>This vector after it has been updated</returns>
		public Vector MultiplyPolarVector(double Length, double Angle) {
			cPolar.Length *= Length;
			cPolar.Angle += Angle;

			ConvertPolarToRect();

			return this;
		}

		/// <summary>
		/// Multiply a vector in rectangular notation to this vector
		/// </summary>
		/// <param name="Real">Real component of the vector to multiply</param>
		/// <param name="Imaginary">Imaginary component of the vector to multiply</param>
		/// <returns>This vector after it has been updated</returns>
		public Vector MultiplyRectangularVector(double Real, double Imaginary) {
			cRect.Real = (cRect.Real * Real) - (cRect.Imaginary * Imaginary);
			cRect.Imaginary = (cRect.Real * Imaginary) + (cRect.Imaginary * Real);

			ConvertRectToPolar();

			return this;
		}

		/// <summary>
		/// Multiply a vector to this vector
		/// </summary>
		/// <param name="MultVector">Vector to multiply</param>
		/// <returns>This vector after it has been updated</returns>
		public Vector MultiplyVector(Vector MultVector) {
			cPolar.Length *= MultVector.Polar.Length;
			cPolar.Angle += MultVector.Polar.Angle;

			ConvertPolarToRect();

			return this;
		}

		/// <summary>
		/// Updates cRect by converting the vector stored in cPolar
		/// </summary>
		private void ConvertPolarToRect() {
			cRect.Real = Math.Cos(DegreesToRadians(cPolar.Angle)) * cPolar.Length;
			cRect.Imaginary = Math.Sin(DegreesToRadians(cPolar.Angle)) * cPolar.Length;
		}

		/// <summary>
		/// Updates cRect by converting the vector information passed in the parameters
		/// </summary>
		/// <param name="Length">Length of the vector</param>
		/// <param name="Angle">Angle of the vector</param>
		private void ConvertPolarToRect(double Length, double Angle) {
			cRect.Real = Math.Cos(DegreesToRadians(Angle)) * Length;
			cRect.Imaginary = Math.Sin(DegreesToRadians(Angle)) * Length;
		}

		/// <summary>
		/// Updates cPolar by converting the vector stored in cRect
		/// </summary>
		private void ConvertRectToPolar() {
			cPolar.Length = Math.Sqrt((cRect.Real * cRect.Real) + (cRect.Imaginary * cRect.Imaginary));
			cPolar.Angle = Math.Atan2(cRect.Imaginary, cRect.Real);
			cPolar.Angle = RadiansToDegrees(cPolar.Angle);
		}

		/// <summary>
		/// Updates cPolar by converting the vector information passed in the parameters
		/// </summary>
		/// <param name="Real">Real component of the vector</param>
		/// <param name="Imaginary">Imaginary component of the vector</param>
		private void ConvertRectToPolar(double Real, double Imaginary) {
			cPolar.Length = Math.Sqrt((Real * Real) + (Imaginary * Imaginary));
			cPolar.Angle = Math.Atan2(Imaginary, Real);
			cPolar.Angle = RadiansToDegrees(cPolar.Angle);
		}

		private double DegreesToRadians(double Degrees) {
			return Degrees * (Math.PI / 180.0);
		}

		private double RadiansToDegrees(double Radians) {
			return Radians * (180.0 / Math.PI);
		}
	}
}
