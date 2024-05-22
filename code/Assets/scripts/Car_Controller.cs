using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Car_Controller : MonoBehaviour
{
    //Public Variables
    [Header("Wheel Colliders")]
    public List<WheelCollider> Front_Wheels; //The front wheels (Wheel Colliders)
    public List<WheelCollider> Back_Wheels; //The rear wheels (Wheel Colliders)

    [Space(10)]

    [Header("Wheel Transforms")]
    public List<Transform> Front_Wheel_Transforms; //The front wheel transforms
    public List<Transform> Back_Wheel_Transforms; //The rear wheel transforms

    [Space(10)]

    [Header("Wheel Transforms Rotations")]
    public List<Vector3> Front_Wheel_Rotation; //The front wheel rotation Vectors
    public List<Vector3> Back_Wheel_Rotation; //The rear wheel rotation Vectors

    [Space(15)]

    [Header("Car Settings")]
    public float Motor_Torque = 400; //Motor torque for the car
    public float Max_Steer_Angle = 25f; //The Maximum Steer Angle for the front wheels
    public float  BrakeForce = 150f; //The brake force of the wheels
    public float Maximum_Speed; //The top speed of the car

    [Space(15)]

    public float handBrakeFrictionMultiplier = 2; //The handbrake friction multiplier
    private float handBrakeFriction  = 0.05f; //The handbrake friction
    public float tempo; //Tempo (don't edit this)

    [Space(15)]

    [Header("Car States")]
    public bool Use_Car_States; //Use car states?
    public bool Car_Started; //Car stared?
    public KeyCode Car_Start_Key; //Key to start the car
    public KeyCode Car_Off_Key; //Key to turn car off

    [Space(15)]

    [Header("Audio Settings")]

    public bool Enable_Engine_Audio; //use engine audio?
    public AudioSource Engine_Sound; //Audio source for engine sound
    public float Minimum_Pitch_Value; //Minimum pitch value for engine
    public float Maximum_Pitch_Value; //Maximum pitch value for engine

    [Space(15)]

    public bool Enable_Horn; //Use horn?
    public AudioSource Horn_Source; //Audio source for horn
    public KeyCode Car_Horn_Key; //Key to use horn

    [Space(15)]

    [Header("Light Setting(s)")]

    [Header("Light Settings (With Light Objects)")]
    public bool Enable_Headlights_Lights; //Enable headlights? (These are light objects)
    public bool Enable_Brakelights_Lights; //Enable brakelights? (These are light objects)
    public bool Enable_Reverselights_Lights; //Enable reverse lights? (These are light objects)
    public KeyCode Headlights_Key; //Key to turn on headlight(s)
    

    public Light[] HeadLights; //Headlight object(s) list/array
    public Light[] BrakeLights; //Brakelight object(s) list/array
    public Light[] ReverseLights; //Reverse light object(s) list/array
    
    
    [Space(15)]

    [Header("Light Setting (By Changing Materials)")]


    public Material HeadLights_Off_Material; //Material when headlights are off
    public Material BrakeLights_Off_Material; //Material when brakelights are off
    public Material ReverseLights_Off_Material; //Material when reverse lights are off

    public Material HeadLights_On_Material; //Material when headlights are on
    public Material BrakeLights_On_Material; //Material when brakelights are on
    public Material ReverseLights_On_Material; //Material when reverse lights are on

    public GameObject[] Headlight_Objects; //Headlight Gameobjects
    public GameObject[] BrakeLight_Objects; //BrakeLight Gameobjects
    public GameObject[] Reverse_Light_Objects; //Reverse Light Gameobjects

    [Space(15)]

    [Header("Other Settings")]
    public Transform Center_of_Mass; //Centre of mass of car
    public  float frictionMultiplier = 3f; //Friction Multiplier
    public Rigidbody Car_Rigidbody; //Car rigidbody

    [Space(15)]

    [Header("Debug Values")]
    public float Car_Speed_KPH; //The car speed in KPH
    public float Car_Speed_MPH; //The car speed in MPH
    
    [Space(15)]

    public bool HeadLights_On; //Headlights on/off?

    //Debug Values in Int Form
    public int Car_Speed_In_KPH; //Car speed in KPH (integer form)
    public int Car_Speed_In_MPH; //Car speed in MPH (integer form)

    public bool Is_Flying () //bool for if the car is flying or not
	{
		if (!Back_Wheels[0].isGrounded && !Front_Wheels[0].isGrounded) {
			return true;
		} else
			return false;
	}

    //private Variables
    private Rigidbody rb; //The rb
    private float Brakes = 0f; //Brakes
    private WheelFrictionCurve  Wheel_forwardFriction, Wheel_sidewaysFriction; //Wheel friction curve(s)

    private Material Headlight_Mat; //Headlight GameObject Material
    private Material BrakeLight_Mat; //Brake Light GameObject Material
    private Material ReverseLight_Mat; //Reverse Light GameObject Material

    //Private Audio Variables
    private float pitch; //Pitch

    //Hidden Variables (not private, but hidden in inspector)
    [HideInInspector] public float currSpeed; //Current speed

    void Start(){
        //To Prevent The Car From Toppling When Turning Too Much
        rb = GetComponent<Rigidbody>(); //get rigidbody
        rb.centerOfMass = Center_of_Mass.localPosition; //Set the centre of mass of the rigid body to the centre of mass transform
        
        //Here we just set the lights to turn on and off at play.

        //We turn the headlights on/off here
        Turn_Off_Headlights();
        Turn_On_Headlights();

        //Here we turn the reverse light(s) off
        if(Enable_Reverselights_Lights){
            foreach(Light R in ReverseLights){
                R.enabled = false;
            }
        }


        //Here we turn off the brakelights
        Turn_Off_Brakelights();

        //Turning some things off if their options are disabled
        if(!Enable_Horn && Horn_Source != null){
            Horn_Source.gameObject.SetActive(false); //is horn is not enabled and the horn source there, disable the horn
        }

        if(!Enable_Engine_Audio && Engine_Sound != null){
            Engine_Sound.gameObject.SetActive(false); //Disable the engine sound if the engine sound has not been enabled and it is set to some audio source.
        }

        if(Engine_Sound != null || Horn_Source != null){
            Horn_Source.gameObject.SetActive(false); 
            Engine_Sound.gameObject.SetActive(false);
        }
    }

    public void FixedUpdate(){
        //Turning car off
        if(Input.GetKeyDown(Car_Off_Key) && (Car_Speed_KPH >= 0 && Car_Speed_KPH <= 1.5f) && Use_Car_States){ //if the car off key has been pressed and the car speed is 0 and the "use car states" is true
            Turn_Off_Car(); //Turn car off
        }

        //Turning Car on
        if(Input.GetKeyDown(Car_Start_Key) && Use_Car_States){ //if the "use car states" is true and that the car start key is pressed
            Car_Started = true;
        }

        //If the car states are not in use
        if(!Use_Car_States){
            Car_Started = true;
        }

        //Check the keys for headlights and turn them off/on
        if(Input.GetKeyDown(Headlights_Key) && Car_Started == true){ //if the headlights key was pressed
            if(!HeadLights_On){
                HeadLights_On = true; //set the headlights on to true
                Turn_On_Headlights(); //turn on headlights
            } else {
                HeadLights_On = false; //Set the headlights on to false
                Turn_Off_Headlights(); //turn off the headlights
            }
        }

        if(Car_Started == false){ //if the car is off
            Turn_Off_Headlights();//turn the headlights off
        }

        //Applying Maximum Speed
        if(Car_Speed_In_KPH < Maximum_Speed && Car_Started){ //if the car's current speed is less than the maximum speed
            //Let car move forward and backward
            foreach(WheelCollider Wheel in Back_Wheels){
                Wheel.motorTorque = Input.GetAxis("Vertical") * ((Motor_Torque * 5)/(Back_Wheels.Count + Front_Wheels.Count));
            }
        }

        if(Car_Speed_In_KPH > Maximum_Speed && Car_Started){ //if the car's current speed is more than the top speed
            //Don't let the car accelerate anymore so it does not exceed the maximum speed
            foreach(WheelCollider Wheel in Back_Wheels){
                Wheel.motorTorque = 0;
            }
        }

        //Making The Car Turn/Steer
        if(Car_Started){
            foreach(WheelCollider Wheel in Front_Wheels){
                Wheel.steerAngle = Input.GetAxis("Horizontal") * Max_Steer_Angle; //Turn the wheels
            }
        }

        //Changing speed of the car
        Car_Speed_KPH = Car_Rigidbody.velocity.magnitude * 3.6f; //Calculate car speed in KPH
        Car_Speed_MPH = Car_Rigidbody.velocity.magnitude * 2.237f; //Calculate the car's speed in MPH

        Car_Speed_In_KPH = (int) Car_Speed_KPH; //Convert the float values of the speed to int
        Car_Speed_In_MPH = (int) Car_Speed_MPH; //Convert the float values of the speed to int

        //Make Car Drift
        WheelHit wheelHit;

        foreach(WheelCollider Wheel in Back_Wheels){
            Wheel.GetGroundHit(out wheelHit);

            if(wheelHit.sidewaysSlip < 0 )	
                tempo = (1 + -Input.GetAxis("Horizontal")) * Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier);

                if(tempo < 0.5) tempo = 0.5f;

            if(wheelHit.sidewaysSlip > 0 )	
                tempo = (1 + Input.GetAxis("Horizontal") )* Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier);

                if(tempo < 0.5) tempo = 0.5f;

            if(wheelHit.sidewaysSlip > .99f || wheelHit.sidewaysSlip < -.99f){
                handBrakeFriction = tempo * 3;
                float velocity = 0;
                handBrakeFriction = Mathf.SmoothDamp(handBrakeFriction,tempo* 3,ref velocity ,0.1f * Time.deltaTime);
                }

            else{
                handBrakeFriction = tempo;
            }
        }

        foreach(WheelCollider Wheel in Front_Wheels){
            Wheel.GetGroundHit(out wheelHit);

            if(wheelHit.sidewaysSlip < 0 )	
                tempo = (1 + -Input.GetAxis("Horizontal")) * Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier);

                if(tempo < 0.5) tempo = 0.5f;

            if(wheelHit.sidewaysSlip > 0 )	
                tempo = (1 + Input.GetAxis("Horizontal") )* Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier);

                if(tempo < 0.5) tempo = 0.5f;

            if(wheelHit.sidewaysSlip > .99f || wheelHit.sidewaysSlip < -.99f){
                //handBrakeFriction = tempo * 3;
                float velocity = 0;
                handBrakeFriction = Mathf.SmoothDamp(handBrakeFriction,tempo* 3,ref velocity ,0.1f * Time.deltaTime);
                }

            else{
                handBrakeFriction = tempo;
            }
        }

        if((Input.GetAxis("Vertical") < 0) && Car_Started){ //Turn on the reverse lights when car is reversing
            //Turn on reverse light(s)
            Turn_On_ReverseLights();
        }

        if((Input.GetAxis("Vertical") > 0) && Car_Started){
            //Turn off reverse light(s)
            Turn_Off_ReverseLights();
        }
    }

    public void Update(){
        

        //Rotating The Wheels Meshes so they have the same position and rotation as the wheel colliders
        var pos = Vector3.zero; //position value (temporary)
        var rot = Quaternion.identity; //rotation value (temporary)
        
        for (int i = 0; i < (Back_Wheels.Count); i++)
        {
            Back_Wheels[i].GetWorldPose(out pos, out rot); //get the world rotation & position of the wheel colliders
            Back_Wheel_Transforms[i].position = pos; //Set the wheel transform positions to the wheel collider positions
            Back_Wheel_Transforms[i].rotation = rot * Quaternion.Euler(Back_Wheel_Rotation[i]); //Rotate the wheel transforms to the rotation of the wheel collider(s) and the rotation offset
        }

        for (int i = 0; i < (Front_Wheels.Count); i++)
        {
            Front_Wheels[i].GetWorldPose(out pos, out rot); //get the world rotation & position of the wheel colliders
            Front_Wheel_Transforms[i].position = pos; //Set the wheel transform positions to the wheel collider positions
            Front_Wheel_Transforms[i].rotation = rot * Quaternion.Euler(Front_Wheel_Rotation[i]); //Rotate the wheel transforms to the rotation of the wheel collider(s) and the rotation offset
        }

        //Make Car Brake
        if(Input.GetKey(KeyCode.Space) && Car_Started){
            Brakes = BrakeForce;

            Turn_On_Brakelights();
            
                foreach(WheelCollider Wheel in Back_Wheels){
                    Wheel_forwardFriction = Wheel.forwardFriction;
                    Wheel_sidewaysFriction = Wheel.sidewaysFriction;

                    Wheel_forwardFriction.extremumValue = Wheel_forwardFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;
                    Wheel_sidewaysFriction.extremumValue = Wheel_sidewaysFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;
                }

                foreach(WheelCollider Wheel in Front_Wheels){
                    Wheel_forwardFriction = Wheel.forwardFriction;
                    Wheel_sidewaysFriction = Wheel.sidewaysFriction;

                    Wheel_forwardFriction.extremumValue = Wheel_forwardFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;
                    Wheel_sidewaysFriction.extremumValue = Wheel_sidewaysFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;
                }
            

            
        } else {
            Brakes = 0f;
        }

        //Apply brake force
        foreach(WheelCollider Wheel in Front_Wheels){
            Wheel.brakeTorque = Brakes; //set the brake torque of the wheels to the brake torque
        }

        foreach(WheelCollider Wheel in Back_Wheels){
            Wheel.brakeTorque = Brakes; //set the brake torque of the wheels to the brake torque
        }

        //Turn the brakelights on
        if(!Input.GetKey(KeyCode.Space) && Car_Started){ //When the car brake button is pressed
            Turn_Off_Brakelights();
        }

        //Audio System
        if(Enable_Engine_Audio && Car_Started){
            //Setting the pitch according to the speed of the car.
            pitch = Car_Speed_In_KPH/Maximum_Speed + 1f;
                
            //Do this if the pitch variable exceeds the maximum pitch value
            if(pitch > Maximum_Pitch_Value){
                pitch = Maximum_Pitch_Value;
            }

            //Do this if the pitch variable is lower than the minimum pitch value
            else if(pitch < Minimum_Pitch_Value){
                pitch = Minimum_Pitch_Value;
            }

            //This actually sets the audio source pitch
            Engine_Sound.pitch = pitch;
        }

        if(Enable_Engine_Audio && !Car_Started){
            //Stop Engine
            Engine_Sound.Stop();
        }

        //Car Horn
        if(Enable_Horn){
            if(Input.GetKey(Car_Horn_Key) && !Horn_Source.isPlaying){
                //Play the sound
                Horn_Source.Play();
            }

            if(!Input.GetKey(Car_Horn_Key)){
                //Stop playing the sound
                Horn_Source.Stop();
            }
        }
        
    }

   

    //Functions to turn on/off the brakelights

    public void Turn_On_Brakelights(){
        //When using lights
        if(Enable_Brakelights_Lights){
            foreach(Light L in BrakeLights){
                L.enabled = true;
            }
            foreach (GameObject G in BrakeLight_Objects)
            {
                BrakeLight_Mat = G.GetComponent<Renderer>().material; //Fetch material of the brake light Object
                BrakeLight_Mat = BrakeLights_On_Material; //Set the brake light object material to the material specified
            }
        }
    }

    public void Turn_Off_Brakelights(){
        //When using lights
        if(Enable_Brakelights_Lights){
            foreach(Light L in BrakeLights){
                L.enabled = false;
            }
            foreach (GameObject G in BrakeLight_Objects)
            {
                BrakeLight_Mat = G.GetComponent<Renderer>().material; //Fetch material of the brake light Object
                BrakeLight_Mat = BrakeLights_Off_Material; //Set the brake light object material to the material specified
            }
        }
    }

    //These are funtions for turning the headlights on & off (so I dont copy/paste the same thing again and again)

    public void Turn_On_Headlights(){
        //Headlights when using lights
        if(Enable_Headlights_Lights){
            foreach(Light H in HeadLights){
                H.enabled = true;
            }
            foreach (GameObject G in Headlight_Objects)
            {
                Headlight_Mat = G.GetComponent<Renderer>().material; //Fetch material of the headlight Object
                Headlight_Mat = HeadLights_On_Material; //Set the headlight object material to the material specified
            }
        }
    }

    public void Turn_Off_Headlights(){
        if(Enable_Headlights_Lights){
            foreach(Light H in HeadLights){
                H.enabled = false;
            }
            foreach (GameObject G in Headlight_Objects)
            {
                Headlight_Mat = G.GetComponent<Renderer>().material; //Fetch material of the headlight Object
                Headlight_Mat = HeadLights_Off_Material; //Set the headlight object material to the material specified
            }
        }  
    }

    //Turn off/on reverse lights functions
    public void Turn_Off_ReverseLights(){
        //When using Light objects
        if(Enable_Reverselights_Lights){
            foreach(Light Rl in ReverseLights){
                Rl.enabled = false;
                foreach (GameObject G in Reverse_Light_Objects)
                {
                    ReverseLight_Mat = G.GetComponent<Renderer>().material; //Fetch material of the headlight Object
                    ReverseLight_Mat = ReverseLights_Off_Material; //Set the headlight object material to the material specified
                }
            }
        }
    }

    public void Turn_On_ReverseLights(){
        //When using light objects
        if(Enable_Reverselights_Lights){
            foreach(Light Rl in ReverseLights){
                Rl.enabled = true;
            }
            foreach (GameObject G in Reverse_Light_Objects)
            {
                ReverseLight_Mat = G.GetComponent<Renderer>().material; //Fetch material of the headlight Object
                ReverseLight_Mat = ReverseLights_On_Material; //Set the headlight object material to the material specified
            }
        }
    }

    //Turn off car function
    public void Turn_Off_Car(){
        Turn_Off_Headlights();
        Turn_Off_Brakelights();
        Turn_Off_ReverseLights();
        Car_Started = false;
    }

    //Function for setting wheel stiffness (not used, just for your own scripts)
    public void Set_Stiffness(float Stiffness_Value){
        Wheel_forwardFriction.stiffness = Stiffness_Value;
        Wheel_forwardFriction.stiffness = Stiffness_Value;
    }

    
}
