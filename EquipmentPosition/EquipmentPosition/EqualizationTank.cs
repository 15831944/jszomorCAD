﻿using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;

namespace EquipmentPosition
{
  public class EqualizationTank : EquipmentBase
  {
    //const double _pumpDistance = 20.0d;
    const double _endPadding = 40.0d;
    const double _startPadding = 50.0d;

    public int NumberOfPumps { get; private set; }
    public int DistanceOfPump { get; set; }
    public short PromptEqIndex { get; set; }

    public EqualizationTank(int numberOfPumps, int distanceOfPump, short eqIndex)
    {
      NumberOfPumps = numberOfPumps;
      DistanceOfPump = distanceOfPump;
      PromptEqIndex = eqIndex;
    }

    public override string EquipmentName => "Equalization Tank";

    public override IEnumerable<InsertBlockBase> Blocks
    {
      get
      {
        #region Chamber
        yield return 
        new InsertBlockBase(
          numberOfItem: 1,
          blockName: "chamber",
          layerName: "unit",
          x: 0,
          y: 0,          
          //rotation: DegreeHelper.DegreeToRadian(0),
          hostName: EquipmentName,
          equipmentStateProperty: new EquipmentStateProperty("Visibility", "no channel"))
          {
          ActionToExecuteOnAttRef = new Action<AttributeReference>[]
          {
            ar =>
            {
              //text for EQ tank - Attributes
              if (ar.Tag == "NAME1")
                ar.TextString = "EQUALIZATION";
              if (ar.Tag == "NAME2")
                ar.TextString = "TANK";
            }
          },
          ActionToExecuteOnDynPropAfter = new Action<DynamicBlockReferenceProperty>[]
          {
            dbrProp =>
            {
              //setup chamber width
              if (dbrProp.PropertyName == "Distance")
                dbrProp.Value = (NumberOfPumps - 1) * DistanceOfPump + _endPadding * 2; //last value is the free space for other items
              //text position for chamber
              if (dbrProp.PropertyName == "Position X")
                dbrProp.Value = ((NumberOfPumps - 1) * DistanceOfPump + _endPadding * 2) / 2.0d; //place text middle of chamber horizontaly 
            }
          },
        };
        #endregion

        #region Eq Pump
        yield return 
        new InsertBlockBase(
          numberOfItem: NumberOfPumps,
          blockName: "pump",
          layerName: "equipment",
          x: _startPadding,
          y: 10.0d,
          //rotation: DegreeHelper.DegreeToRadian(0),
          hostName: EquipmentName,
          equipmentStateProperty: new EquipmentStateProperty("Centrifugal Pump", PromptEqIndex))
          {
          OffsetX = DistanceOfPump, // distamce between pumps
          ActionToExecuteOnAttRef = null,
          ActionToExecuteOnDynPropAfter = new Action<DynamicBlockReferenceProperty>[]
          {
            dbrProp =>
            {
              //for pumps VFD rotate
              if (dbrProp.PropertyName == "Angle1")
                dbrProp.Value = DegreeHelper.DegreeToRadian(90);

              // pumps VFD rotate
              if (dbrProp.PropertyName == "Angle2")
                dbrProp.Value = DegreeHelper.DegreeToRadian(270);
            }
          },
        };
        #endregion

        #region Jet Pump
        yield return
        new InsertBlockBase(
          numberOfItem: 1,
          blockName: "pump",
          layerName: "equipment",
          x: 20.0d,
          y: 10.0d,
          rotation: DegreeHelper.DegreeToRadian(270),
          hostName: EquipmentName,
          equipmentStateProperty: new EquipmentStateProperty("Centrifugal Pump", (short)17))
          {
          ActionToExecuteOnDynPropAfter = new Action<DynamicBlockReferenceProperty>[]
          {
            dbrProp =>
            {              
              // tag horizontal positioning
              if (dbrProp.PropertyName == "Angle")
                dbrProp.Value = DegreeHelper.DegreeToRadian(90);
              if (dbrProp.PropertyName == "Position X")
                dbrProp.Value = 6.0d;
              if (dbrProp.PropertyName == "Position Y")
                dbrProp.Value = 0.0d;
            }
          },
        };
        #endregion

        #region Check Valve
        yield return
        new InsertBlockBase(
          numberOfItem: NumberOfPumps,
          blockName: "valve",
          layerName: "valve",
          x: _startPadding,
          y: 25.0d,
          rotation: DegreeHelper.DegreeToRadian(90),
          hostName: EquipmentName,
          equipmentStateProperty: new EquipmentStateProperty("Block Table1", (short)5))
          {
          OffsetX = DistanceOfPump,
          ActionToExecuteOnDynPropAfter = new Action<DynamicBlockReferenceProperty>[]
          {
            dbrProp =>
            {              
              // flip
              if (dbrProp.PropertyName == "Flip state")                
                dbrProp.Value = (short)0;
              if (dbrProp.PropertyName == "Flip state1")
                dbrProp.Value = (short)0;
            }
          },
        };
        #endregion

        #region Gate Valve
        yield return
        new InsertBlockBase(
          numberOfItem: NumberOfPumps,
          blockName: "valve",
          layerName: "valve",
          x: _startPadding,
          y: 40.0d,
          rotation: DegreeHelper.DegreeToRadian(90),
          hostName: EquipmentName,
          equipmentStateProperty: new EquipmentStateProperty("Block Table1", (short)0))
          {
          OffsetX = DistanceOfPump,
          ActionToExecuteOnDynPropAfter = new Action<DynamicBlockReferenceProperty>[]
          {
            dbrProp =>
            {              
              // symbol flip
              if (dbrProp.PropertyName == "Flip state")
                dbrProp.Value = (short)0;
              if (dbrProp.PropertyName == "Flip state1")
                dbrProp.Value = (short)0;
            }
          },
        };
        #endregion

        #region LIT instrument
        yield return
        new InsertBlockBase(
          numberOfItem: 1,
          blockName: "instrumentation tag",
          layerName: "instrumentation",
          x: NumberOfPumps * DistanceOfPump + 50,
          y: 10.0d,
          //rotation: DegreeHelper.DegreeToRadian(0),
          hostName: EquipmentName,
          equipmentStateProperty: new EquipmentStateProperty("Block Table1", (short)7))
          {          
          ActionToExecuteOnDynPropAfter = new Action<DynamicBlockReferenceProperty>[]
          {
            dbrProp =>
            { 
              //line angle
              if (dbrProp.PropertyName == "Angle")
                dbrProp.Value = DegreeHelper.DegreeToRadian(180);
              //line length
              if (dbrProp.PropertyName == "Distance")
                dbrProp.Value = (double)11;
            }
          },
        };
        #endregion

        #region FIT instrument
        yield return
        new InsertBlockBase(
          numberOfItem: 1,
          blockName: "instrumentation tag",
          layerName: "instrumentation",
          x: NumberOfPumps * DistanceOfPump + 50,
          y: 50.0d,
          //rotation: DegreeHelper.DegreeToRadian(0),
          hostName: EquipmentName,
          equipmentStateProperty: new EquipmentStateProperty("Block Table1", (short)11))
          {
          ActionToExecuteOnDynPropAfter = new Action<DynamicBlockReferenceProperty>[]
          {
            dbrProp =>
            { 
              //line angle
              if (dbrProp.PropertyName == "Angle")
                dbrProp.Value = DegreeHelper.DegreeToRadian(180);
              //line length
              if (dbrProp.PropertyName == "Distance")
                dbrProp.Value = (double)11;
            }
          },
        };
        #endregion

        #region ChannelGateDuty
        yield return
        new InsertBlockBase(
          numberOfItem: 1,
          blockName: "channel gate",
          layerName: "equipment",
          x: -10.5,
          y: -24,
          //rotation: DegreeHelper.DegreeToRadian(0),
          hostName: EquipmentName,
          equipmentStateProperty: new EquipmentStateProperty("Block Table1", (short)23))
          {
          ActionToExecuteOnDynPropAfter = new Action<DynamicBlockReferenceProperty>[]
          {
            
          },
        };
        #endregion

        #region ChannelGateStandby
        yield return
        new InsertBlockBase(
          numberOfItem: 1,
          blockName: "channel gate",
          layerName: "equipment",
          x: -10.5,
          y: 6,
          //rotation: DegreeHelper.DegreeToRadian(0),
          hostName: EquipmentName,
          equipmentStateProperty: new EquipmentStateProperty("Block Table1", (short)23))
          {
          ActionToExecuteOnDynPropAfter = new Action<DynamicBlockReferenceProperty>[]
          {
            
          },
        };
        #endregion

        #region pump pipe
        yield return
        new InsertBlockBase(
          numberOfItem: NumberOfPumps,
          blockName: "pipe",
          layerName: "sewer",
          x: _startPadding,
          y: 14.0d,
          rotation: DegreeHelper.DegreeToRadian(0),
          hostName: EquipmentName,
          equipmentStateProperty: new EquipmentStateProperty("Visibility1", "sewer"))
        {
          OffsetX = DistanceOfPump, // distamce between pipes
          ActionToExecuteOnDynPropAfter = new Action<DynamicBlockReferenceProperty>[]
          {
            dbrProp =>
            {               
              if (dbrProp.PropertyName == "PipeLength")
                dbrProp.Value = (double)36;
              if (dbrProp.PropertyName == "ArrowPosition X")
                dbrProp.Value = (double)0;
              if (dbrProp.PropertyName == "ArrowPosition Y")
                dbrProp.Value = (double)36;
              if (dbrProp.PropertyName == "TagPosition X")
                dbrProp.Value = (double)-5;
              if (dbrProp.PropertyName == "TagPosition Y")
                dbrProp.Value = (double)20;
              if (dbrProp.PropertyName == "TagAngle")
                dbrProp.Value = DegreeHelper.DegreeToRadian(180);
            }
          },
        };
        #endregion
      }
    }
  }
}