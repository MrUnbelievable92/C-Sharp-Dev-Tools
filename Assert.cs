// Constraints:
// No use of the ConditionalAttribute
// strings, Exceptions or any other managed objects cannot be passed as arguments to functions if they are to work with Unity.Burst

#if DEBUG

#define BOOLEAN_CONDITION_CHECKS
#define NULL_CHECKS
#define FILE_PATH_CHECKS
#define ARRAY_BOUNDS_CHECKS
#define COMPARISON_CHECKS
#define ARITHMETIC_LOGIC_CHECKS
#define MEMORY_CHECKS

#endif

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace DevTools
{
    public static partial class Assert
    {
        internal const string ASSERTION_FAILED_TAG = "Assertion Failed: ";


        public static UnreachableException Unreachable()
        {
#if DEBUG
            throw new UnreachableException();
#else
            return null;
#endif
        }

        #region BOOLEAN_CONDITION_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__BOOLEAN_CONDITION_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__BOOLEAN_CONDITION_CHECKS)]
        public static void IsTrue(bool condition)
        {
#if BOOLEAN_CONDITION_CHECKS
            if (!condition)
            {
                throw new Exception(ASSERTION_FAILED_TAG + "Expected 'true'.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__BOOLEAN_CONDITION_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__BOOLEAN_CONDITION_CHECKS)]
        public static void IsFalse(bool condition)
        {
#if BOOLEAN_CONDITION_CHECKS
            if (condition)
            {
                throw new Exception(ASSERTION_FAILED_TAG + "Expected 'false'.");
            }
#endif
        }
        #endregion

        #region NULL_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        public static void IsNull(object obj)
        {
#if NULL_CHECKS
            if (obj != null)
            {
                throw new InvalidDataException(ASSERTION_FAILED_TAG + "Expected null.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        public static void IsNull<T>(T? obj)
            where T : struct
        {
#if NULL_CHECKS
            if (obj != null)
            {
                throw new InvalidDataException(ASSERTION_FAILED_TAG + "Expected null.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        unsafe public static void IsNull(void* ptr)
        {
#if NULL_CHECKS
            if (ptr != null)
            {
                throw new InvalidDataException(ASSERTION_FAILED_TAG + "Expected null.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        public static void IsNotNull(object obj)
        {
#if NULL_CHECKS
            if (obj == null)
            {
                throw new NullReferenceException(ASSERTION_FAILED_TAG + "Expected not-null.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        public static void IsNotNull<T>(T? obj)
            where T : struct
        {
#if NULL_CHECKS
            if (obj == null)
            {
                throw new NullReferenceException(ASSERTION_FAILED_TAG + "Expected not-null.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        unsafe public static void IsNotNull(void* ptr)
        {
#if NULL_CHECKS
            if (ptr == null)
            {
                throw new NullReferenceException(ASSERTION_FAILED_TAG + "Expected not-null.");
            }
#endif
        }
        #endregion

        #region FILE_PATH_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__FILE_PATH_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__FILE_PATH_CHECKS)]
        public static void FileExists(string path)
        {
#if FILE_PATH_CHECKS
            IsNotNull(path); // File.Exists only returns 'false' in case 'path' is null (no explicit throw, which is what I want)

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(ASSERTION_FAILED_TAG + path + " not found");
            }
#endif
        }
        #endregion

        #region ARRAY_BOUNDS_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS)]
        public static void IsWithinArrayBounds(long index, long arrayLength)
        {
#if ARRAY_BOUNDS_CHECKS
            IsNonNegative(arrayLength);

            if ((ulong)index >= (ulong)arrayLength)
            {
                throw new IndexOutOfRangeException(ASSERTION_FAILED_TAG + $"{ index } is out of range (length { arrayLength } - 1).");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS)]
        public static void IsWithinArrayBounds(ulong index, ulong arrayLength)
        {
#if ARRAY_BOUNDS_CHECKS
            if (index >= arrayLength)
            {
                throw new IndexOutOfRangeException(ASSERTION_FAILED_TAG + $"{ index } is out of range (length { arrayLength } - 1).");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS)]
        public static void IsValidSubarray(int index, int numEntries, int arrayLength)
        {
#if ARRAY_BOUNDS_CHECKS
            IsWithinArrayBounds(index, arrayLength);
            IsNonNegative(numEntries);

            if (index + numEntries > arrayLength)
            {
                throw new IndexOutOfRangeException(ASSERTION_FAILED_TAG + $"{ nameof(index) } + { nameof(numEntries) } is { index + numEntries }, which is larger than length { arrayLength }.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS)]
        public static void SubarraysDoNotOverlap(int firstIndex, int secondIndex, int firstNumEntries, int secondNumEntries)
        {
#if ARRAY_BOUNDS_CHECKS
            if (firstIndex < secondIndex)
            {
                if (firstIndex + firstNumEntries > secondIndex)
                {
                    throw new IndexOutOfRangeException(ASSERTION_FAILED_TAG + $"Subarray from { firstIndex } to { firstIndex + firstNumEntries - 1} overlaps with subarray from { secondIndex } to { secondIndex + secondNumEntries - 1 }.");
                }
            }
            else
            {
                if (secondIndex + secondNumEntries > firstIndex)
                {
                    throw new IndexOutOfRangeException(ASSERTION_FAILED_TAG + $"Subarray from { secondIndex } to { secondIndex + secondNumEntries - 1} overlaps with subarray from { firstIndex } to { firstIndex + firstNumEntries - 1 }.");
                }
            }
#endif
        }
        #endregion

        #region COMPARISON_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsPositive(long value)
        {
#if COMPARISON_CHECKS
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be positive.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsPositive(float value)
        {
#if COMPARISON_CHECKS
            if (value <= 0f)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be positive.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsPositive(double value)
        {
#if COMPARISON_CHECKS
            if (value <= 0d)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be positive.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsPositive(decimal value)
        {
#if COMPARISON_CHECKS
            if (value <= 0m)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be positive.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNegative(long value)
        {
#if COMPARISON_CHECKS
            if (value >= 0)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be negative.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNegative(float value)
        {
#if COMPARISON_CHECKS
            if (value >= 0f)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be negative.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNegative(double value)
        {
#if COMPARISON_CHECKS
            if (value >= 0d)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be negative.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNegative(decimal value)
        {
#if COMPARISON_CHECKS
            if (value >= 0m)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be negative.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNonNegative(long value)
        {
#if COMPARISON_CHECKS
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be positive or equal to zero.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNonNegative(float value)
        {
#if COMPARISON_CHECKS
            if (value < 0f)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be positive or equal to zero.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNonNegative(double value)
        {
#if COMPARISON_CHECKS
            if (value < 0d)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be positive or equal to zero.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNonNegative(decimal value)
        {
#if COMPARISON_CHECKS
            if (value < 0m)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be positive or equal to zero.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotPositive(long value)
        {
#if COMPARISON_CHECKS
            if (value > 0)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be negative or equal to zero.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotPositive(float value)
        {
#if COMPARISON_CHECKS
            if (value > 0f)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be negative or equal to zero.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotPositive(double value)
        {
#if COMPARISON_CHECKS
            if (value > 0d)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be negative or equal to zero.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotPositive(decimal value)
        {
#if COMPARISON_CHECKS
            if (value > 0m)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be negative or equal to zero.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void AreEqual<T>(T a, T b)
            where T : IEquatable<T>
        {
#if COMPARISON_CHECKS
            if (!a.Equals(b))
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ a } was expected to be equal to { b }.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void AreNotEqual<T>(T a, T b)
            where T : IEquatable<T>
        {
#if COMPARISON_CHECKS
            if (a.Equals(b))
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ a } was expected not to be equal to { b }.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       The comparison is inclusive.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsBetween<T>(T value, T min, T max)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if ((value.CompareTo(min) < 0) || (value.CompareTo(max) > 0))
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"Min: { min }, Max: { max }, Value: { value }.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsSmallerOrEqual<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) > 0)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be smaller than or equal to { limit }.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsSmaller<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) > -1)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be smaller than { limit }.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsGreaterOrEqual<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) < 0)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be greater than or equal to { limit }.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsGreater<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) < 1)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected to be greater than { limit }.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotSmaller<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) < 0)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected not to be smaller than { limit }.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotGreater<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) > 0)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"{ value } was expected not to be greater than { limit }.");
            }
#endif
        }
        #endregion

        #region ARITHMETIC_LOGIC_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS)]
        unsafe public static void IsSafeBoolean(bool x)
        {
#if ARITHMETIC_LOGIC_CHECKS
            if (*(byte*)&x > 1)
            {
                throw new InvalidDataException(ASSERTION_FAILED_TAG + $"The numerical value of the bool { nameof(x) } is { *(byte*)&x } which can lead to undefined behavior.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS)]
        unsafe public static void IsDefinedBitShift<T>(int amount)
            where T : unmanaged
        {
#if ARITHMETIC_LOGIC_CHECKS
            if ((uint)amount >= (uint)sizeof(T) * 8u)
            {
                throw new ArgumentOutOfRangeException(ASSERTION_FAILED_TAG + $"Shifting a { typeof(T) } by { amount } results in undefined behavior.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS)]
        public static void IsDefinedBitShift<T>(uint amount)
            where T : unmanaged
        {
            IsDefinedBitShift<T>((int)amount);
        }
        #endregion

        #region MEMORY_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__MEMORY_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__MEMORY_CHECKS)]
        unsafe public static void AreNotAliased(void* ptrA, long bytesA, void* ptrB, long bytesB)
        {
#if MEMORY_CHECKS
            bool overlap = (ulong)ptrA < (ulong)ptrB && ((ulong)ptrA + (ulong)bytesA - 1 >= (ulong)ptrB);
            overlap |= (ulong)ptrB < (ulong)ptrA && ((ulong)ptrB + (ulong)bytesB - 1 >= (ulong)ptrA);
            overlap |= (ulong)ptrB == (ulong)ptrA;

            if (overlap)
            {
                throw new MemberAccessException(ASSERTION_FAILED_TAG + $"The memory block, associated with the base address { Dump.Hex((ulong)ptrA) } and byte length { bytesA } overlaps with the region in memory associated with the base address {Dump.Hex((ulong)ptrB) } and byte length {bytesB }.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__MEMORY_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__MEMORY_CHECKS)]
        unsafe public static void AreNotAliased<T, U>(T* ptrA, long lengthA, U* ptrB, long lengthB)
            where T : unmanaged
            where U : unmanaged
        {
            AreNotAliased((void*)ptrA, sizeof(T) * lengthA, (void*)ptrB, sizeof(U) * lengthB);
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__MEMORY_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__MEMORY_CHECKS)]
        unsafe public static void IsMemoryAligned<T>(T* ptr)
            where T : unmanaged
        {
#if MEMORY_CHECKS
            switch (sizeof(T))
            {
                case 2:
                case 4:
                case 8:
                case 16:
                case 32:
                case 64:
                {
                    if ((ulong)ptr % (uint)sizeof(T) != 0)
                    {
                        throw new DataMisalignedException(ASSERTION_FAILED_TAG + $"The address { Dump.Hex((ulong)ptr) } of a { typeof(T) } of size { sizeof(T) } is misaligned by { (ulong)ptr % (uint)sizeof(T) } bytes.");
                    }

                    return;
                }

                default: return;
            }
#endif
        }
        #endregion
    }
}