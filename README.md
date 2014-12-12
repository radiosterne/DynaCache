DynaCache.Extended
=========

DynaCache.Extended is a better version of DynaCache (https://dynacache.codeplex.com/), which is small and easy-to-use library for .NET in-memory caching.

DynaCache.Extended provides safer ways of ensuring that your data is cached correctly — while DynaCache blindly assumes that any cacheable method parameter type ToString implementation yields different string values for different objects, 
DynaCache.Extended allows only to cache methods with parameter types from list of predefined types, or types marked with `[ToStringable]` attribute, or types for which a custom converter exists and registered inside DynaCache.

##Usage
**For basic DynaCache usage please refer to https://dynacache.codeplex.com/**
###Registering custom type converter
Consider you want Exception mapped to Exception.Message for caching key creation purposes. 
Then you should create converter function like this: `Func<object, string> converter = e => ((Exception)e).Message;`,
and register it inside DynaCache with `Cacheable.AddCustomConverter<Exception>(converter);`

Important: you should register your converters before creation of any Cacheable types (the best practice is to register all the needed converters before your DI container bootstrapper is run).
