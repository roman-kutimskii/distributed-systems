﻿namespace Valuator.Specs.Fixture;

public class TestServerFixture : ITestServerFixture
{
    public HttpClient HttpClient => TestServerFixtureCore.Instance.HttpClient;
}