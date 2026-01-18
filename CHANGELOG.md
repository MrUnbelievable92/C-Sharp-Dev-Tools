## Fixes

- Fixed `COMPARISON_CHECKS` assertions' `CompareTo` calls to allow for `T` being of the type `byte`, as `byte.CompareTo` does not necessarily only return 0, 1 and -1

## Additions

- Added scoped registry support
- Added `LogState<T>` as a generic extension to any instance of a type, returning a `string` that contains all members of the instance using their `ToString()` methods

## Improvements

- Reduced memory footprint in the Unity Editor slightly
- [Unity Editor] Activating- and Deactivating Code Paths via `Window/C# Dev Tools/Debug Burst Intrinsics` is now multithreaded