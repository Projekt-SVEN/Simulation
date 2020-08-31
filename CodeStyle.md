# Code Style

## Case Notation

UpperCamelCase    
lowerCamelCase    
CAPS_SNAKE_CASE
Upper_Snake_Case    
lower_snake_case    
CAPS

## Language

Comments, Class-names, Variable names, ... (**everything!**) should be written in english.

## class-names, struct-names, enum-names should be written in UpperCamelCase


```C#
public class DemoClass
{
}

public struct DemoStruct
{
}

public enum DemoEnum
{
}
```

## Constants should be written in CAPS_SNAKE_CASE

```C#
public class DemoClass
{
    public const int CONSTANT_VALUE = 42;
}
```

## Class-attributes should be written in lowerCamelCase, prefixed with a "_" and should be private

```C#
public class DemoClass
{
    private static object  _staticClassAttribute;

    private object _classAttribute;
}
```

## local variables should be written in lowerCamelCase

```C#
public class DemoClass
{
    public void DemoMethod()
    {
        object localVariable;
    }
}
```

## enum values should be written in UpperCamelCase

```C#
public enum DemoEnum
{
    EnumValue1,
    EnumValue2
}
```

## single-line if's shouldn't be used

```C#
public class DemoClass
{
    public void DemoMethod()
    {
        //DO:
        if (true)
        {
            Console.WriteLine("Hello World!");
        }

        //DON'T:
        if (true)
            Console.WriteLine("Hello World!");
    }
}
```

## single-line while's shouldn't be used

```C#
public class DemoClass
{
    public void DemoMethod()
    {
        //DO:
        while (true)
        {
            Console.WriteLine("Hello World!");
        }

        //DON'T:
        while (true)
            Console.WriteLine("Hello World!");
    }
}
```

## single-line for's shouldn't be used

```C#
public class DemoClass
{
    public void DemoMethod()
    {
        //DO:
        for (;;)
        {
            Console.WriteLine("Hello World!");
        }

        //DON'T:
        for (;;)
            Console.WriteLine("Hello World!");
    }
}
```

## single-line foreach's shouldn't be used

```C#
public class DemoClass
{
    public void DemoMethod()
    {
        int[] primesBelowTen = new int[] { 2, 3, 5, 7 }

        //DO:
        foreach (int prime in primesBelowTen)
        {
            Console.WriteLine(prime);
        }

        //DON'T:
        foreach (int prime in primesBelowTen)
            Console.WriteLine(prime);
    }
}
```

## methods/functions should be written in UpperCamelCase

```C#
public class DemoClass
{
    public int DemoFunction()
    {
        return 42;
    }

    public void DemoMethod()
    {
        
    }
}
```

## every should have a descriptive name

Exceptions are loop variables like "i","j","k".

```C#
public class DemoClass
{ 
    public void DemoMethod()
    {
        //DO:
        int meaningOfLife = 42;

        //DON'T:
        int temp = 42;    
    }
}
```
