# FakeItEasy.Capture
Capturing arguments sent to a fake.
Supports **.NET Core** (.NET Standard 1.6+)

## Installation
```
PM> Install-Package FakeItEasy.Capture
```

## Usage
### Capture single invocation
```csharp
// Capturing a single argument
var singleArgument = new Capture<string>();
var dependency = A.Fake<SomeDependency>();
A.CallTo(() => dependency.SomeMethod(singleArgument)).DoesNothing();

dependency.SomeMethod("I am captured!");

Console.WriteLine(singleArgument.Value == "I am captured!");
```

### Capture multiple invocations
```csharp
// Capturing multiple arguments
var multipleArguments = new Capture<string>();
var dependency = A.Fake<SomeDependency>();
A.CallTo(() => dependency.SomeMethod(multipleArguments)).DoesNothing();

dependency.SomeMethod("I am captured!");
dependency.SomeMethod("I, too, am captured!");
dependency.SomeMethod("I am also captured!");

Console.WriteLine(multipleArguments.Values.Count == 3);
```

## Caveats
The `Capture` instance can only be used to configure a single call.

Example
```csharp
// Capturing multiple arguments - in multiple passes
var singleArgument = new Capture<string>();
var dependency = A.Fake<IDependency>();
A.CallTo(() => dependency.SomeMethod(singleArgument)).DoesNothing();
A.CallTo(() => dependency.SomeOtherMethod(singleArgument)).DoesNothing(); // This fails
```