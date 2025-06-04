Feature: Text Validation
As a user
I want to validate text using the Valuator component
So that I can ensure the text meets required criteria

    Scenario: Sending text for validation
        Given I have the Valuator component
        When I submit the text "Sample text for validation" for validation
        Then the validation process should be initiated
        And I should receive a validation response