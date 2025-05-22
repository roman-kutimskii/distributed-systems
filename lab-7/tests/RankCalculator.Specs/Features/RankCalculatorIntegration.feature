Feature: RankCalculator Integration
  As a text processing system
  I want to calculate ranks for texts in the processing pipeline
  So that I can determine text quality metrics

  Background:
    Given the RankCalculator service is running

  Scenario: Process text through the rank calculation pipeline
    Given a text with ID "test-text-1" is stored in the database
    And the text content is "Hello, this is a test text with some numbers 123."
    When the rank calculation is triggered for text ID "test-text-1"
    Then the calculated rank should be stored in the database
    And the calculated rank should be approximately 0.2558
    And a rank calculation event should be published
