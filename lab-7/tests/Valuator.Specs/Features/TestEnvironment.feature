Feature: Test Environment
    In order to run tests reliably
    As a developer
    I want to ensure the test environment is properly set up

Scenario: Verify test environment initialization
    Given the test environment is initialized
    When I check the system health
    Then the system should be ready for testing
