namespace ExplicitVsImplicit
{
    // Vehicle Interface
    interface IVehicle
    {
        string Accelerate();
    }

    // Aircraft Interface
    interface IAirCraft
    {
        string Accelerate();
    }

    // Hovercraft 
    class HoverCraft : IAirCraft, IVehicle
    {
        // How should I Accelerate ?

        // Implicit Implementation will confuse me
        string Accelerate() { return "Confused, Crash"; }

        // Explicit Implementation
        string IAirCraft.Accelerate()
        {
            return "Preparing for Lift Off!";
        }

        string IVehicle.Accelerate()
        {
            return "Step on the Gas!";
        }
    }

    
}
