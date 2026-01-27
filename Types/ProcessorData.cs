namespace TileGame.Types;

// Base class for processor-specific data
public abstract class ProcessorData
{
    public abstract string ProcessorType { get; }
}

// Data for MachineProcessor
public class MachineProcessorData : ProcessorData
{
    public override string ProcessorType => "machine";
    public required Dictionary<string, MachineCase> Consumption { get; set; } = [];
    public required Dictionary<string, MachineCase> Production { get; set; } = [];
}

// Represents a single case (e.g., "default") in consumption or production
public class MachineCase
{
    public required Dictionary<string, ContainerSpec> Containers { get; set; } = [];
}

// Specifies which container and what resources for a case
public class ContainerSpec
{
    public required string ContainerName { get; set; }
    public required Dictionary<Resource, int> Resources { get; set; } = [];
}

// Data for ExtractorProcessor
public class ExtractorProcessorData : ProcessorData
{
    public override string ProcessorType => "extractor";
    public required List<ExtractionSpec> Extraction { get; set; } = [];
    public required List<RequirementSpec> Requirements { get; set; } = [];
}

public class ExtractionSpec
{
    public required string Container { get; set; }
    public required int Amount { get; set; }
}

public class RequirementSpec
{
    public required string Container { get; set; }
    public required int Amount { get; set; }
}

// Data for PipeProcessor (when implemented)
public class PipeProcessorData : ProcessorData
{
    public override string ProcessorType => "pipe";
    // Add pipe-specific properties as needed
    public required List<Side> Ports { get; set; } = [];
}
