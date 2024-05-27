# PluginSystemIntegrationTests

Integration tests for the plugin system that validate end-to-end scenarios including event handling, formatter resolution, dependency management, and concurrency.

## API

### `MainUseCase_ValidateResolveAndPublishEvents_FullPipeline`
Validates the complete plugin lifecycle: loading, resolution, event publishing, and unloading. Ensures all registered events are delivered in the correct order and that plugin cleanup occurs without leaks.

### `EventSystem_SubscriberReceivesAllRegisteredEventTypes`
Verifies that a subscriber registered for multiple event types receives each event exactly once when those events are published. Covers event routing and type-based dispatch.

### `EventSystem_SubscriberGetSubscriptionCount_ReflectsRegistrations`
Asserts that the subscription count returned by a subscriber matches the number of event types it has registered for. Validates internal state consistency.

### `EventSystem_UnsubscribeAll_StopsDelivery`
Confirms that invoking `UnsubscribeAll` on a subscriber prevents further delivery of any events. Ensures cleanup and isolation between tests.

### `FormatterFactory_JsonFormatter_ProducesValidOutput`
Tests that the JSON formatter generates syntactically valid JSON output for a given payload. Validates serialization correctness and schema compliance.

### `FormatterFactory_AllSupportedFormats_ProduceOutput`
Ensures that all supported formatters (e.g., JSON, XML) produce non-empty, valid output for a standard payload. Confirms formatter availability and basic functionality.

### `FormatterFactory_UnknownFormat_ReturnsNull`
Checks that requesting an unsupported format returns `null` rather than throwing. Validates graceful degradation and input validation.

### `FormatterFactory_GetSupportedFormats_ContainsExpectedFormats`
Verifies that the list of supported formats includes all expected values (e.g., `json`, `xml`). Ensures configuration and discovery are in sync.

### `Concurrency_SimultaneousValidationsAndResolutions_AllSucceed`
Stress-tests the system under concurrent validation and resolution operations. Confirms thread safety and absence of race conditions in shared state.

### `Configuration_PluginWithOptionalAndRequiredDependencies_ValidatesCorrectly`
Validates plugin configurations with a mix of optional and required dependencies. Ensures dependency resolution respects constraints and optional flags.

### `Configuration_PluginWithVersionConstraints_EnforcesMinimumVersion`
Tests that version constraints (e.g., `>= 1.0.0`) are enforced during dependency resolution. Confirms semantic versioning compliance.

### `EventChain_PublishLoadedThenUpdatedThenUnloaded_AllDelivered`
Validates event delivery across the full plugin lifecycle: loading, update, and unloading. Ensures events are delivered even when plugins are dynamically added or removed.

## Usage
