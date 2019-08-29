﻿using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using EquipmentPosition;
using OrganiCAD.AutoCAD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jszomorCAD
{
  public class InsertBlockTable
  {
    //public void InsertVfdPump(Database db, PromptIntegerResult number, PromptIntegerResult distance, string itemType, string layerName, int eqIndex) => 
    //  InsertBlockTableMethod(db, number, distance, itemType, layerName, "Centrifugal Pump", eqIndex); // todo: magic number

    public void InsertBlockTableMethod(Database db, double number, double distance, string itemType, string layerName, string propertyName, object eqIndex, double X, double Y)
    {      
      Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;     
      var aw = new AutoCadWrapper();
      BlockTableRecord btr;
      var blockDefinitions = new List<ObjectId>();
      var positionProperty = new PositionProperty();

      //setup default layers
      var layerCreator = new LayerCreator();
      layerCreator.LayerCreatorMethod("equipment", Color.FromRgb(0, 0, 255), 0.25);      
      layerCreator.LayerCreatorMethod("unit", Color.FromRgb(255, 0, 0), 0.25);
      layerCreator.LayerCreatorMethod("valve", Color.FromRgb(255, 255, 255), 0.25);
      layerCreator.LayerCreatorMethod("valve2", Color.FromRgb(255, 255, 255), 0.25);
      layerCreator.LayerCreatorMethod("instrumentation", Color.FromRgb(0, 255, 255), 0.25);
      layerCreator.LayerCreatorMethod("text", Color.FromRgb(255, 255, 255), 0.25);
      layerCreator.LayerCreatorMethod("sewer", Color.FromRgb(28, 38, 0), 0.25);
      layerCreator.LayerCreatorMethod("sludge", Color.FromRgb(38, 19, 19), 0.25);
      layerCreator.LayerCreatorMethod("chemical", Color.FromRgb(0, 255, 255), 0.25);
      layerCreator.LayerCreatorMethod("water", Color.FromRgb(0, 0, 255), 0.25);
      layerCreator.LayerCreatorMethod("treated_water", Color.FromRgb(0, 127, 255), 0.25);
      layerCreator.LayerCreatorMethod("air", Color.FromRgb(63, 255, 0), 0.25);
      layerCreator.LayerCreatorMethod("recycle_flow", Color.FromRgb(145, 165, 82), 0.25);

      //var shortEqIndex = Convert.ToInt16(eqIndex);

      // Start transaction to insert equipment
      aw.ExecuteActionOnBlockTable(db, (tr, bt) =>
      {        
        foreach (ObjectId btrId in bt)
        {
          using (btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead, false))
          {
            // Only add named & non-layout blocks to the copy list
            if (!btr.IsAnonymous && !btr.IsLayout && btr.Name == itemType)
            {  
              blockDefinitions.Add(btrId);             
            }
          }
        }

        //X = 0;

        //X = positionProperty.X;
        //Y = positionProperty.Y;

        var currentSpaceId = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

        for (int i = 0; i < number; i++)
        {
          foreach (var objectId in blockDefinitions)
          {
            using (var blockDefinition = (BlockTableRecord)tr.GetObject(objectId, OpenMode.ForRead, false))
            {
              using (var acBlkRef = new BlockReference(new Point3d(X, Y, positionProperty.Z), objectId))
              {
                currentSpaceId.AppendEntity(acBlkRef);
                tr.AddNewlyCreatedDBObject(acBlkRef, true);

                acBlkRef.Layer = layerName;                

                // copy/create attribute references
                foreach (var bdEntityObjectId in blockDefinition)
                {
                  var ad = tr.GetObject(bdEntityObjectId, OpenMode.ForRead) as AttributeDefinition;
                  if (ad == null) continue;

                  var ar = new AttributeReference();
                  ar.SetDatabaseDefaults(db);
                  ar.SetAttributeFromBlock(ad, acBlkRef.BlockTransform);
                  ar.TextString = ad.TextString;
                  ar.AdjustAlignment(HostApplicationServices.WorkingDatabase);

                  acBlkRef.AttributeCollection.AppendAttribute(ar);
                  tr.AddNewlyCreatedDBObject(ar, true);

                  //text for EQ tank - Attributes
                  if (ar.Tag == "NAME1" && itemType == "chamber")
                    ar.TextString = "EQUALIZATION";
                  if (ar.Tag == "NAME2" && itemType == "chamber")
                    ar.TextString = "TANK";

                  //valve setup
                  if(itemType == "valve" && acBlkRef.Name == "Rotation")
                  {
                  }
                }                

                // set dynamic properties
                if (acBlkRef.IsDynamicBlock)
                {
                  foreach (DynamicBlockReferenceProperty dbrProp in acBlkRef.DynamicBlockReferencePropertyCollection)
                  {
                    if (dbrProp.PropertyName == propertyName)
                      dbrProp.Value = eqIndex; // SHORT !!!!!!!!!!!!

                    //for pumps VFD rotate
                    if (dbrProp.PropertyName == "Angle1")
                      dbrProp.Value = DegreeHelper.DegreeToRadian(90);

                    // pumps VFD rotate
                    if (dbrProp.PropertyName == "Angle2")
                      dbrProp.Value = DegreeHelper.DegreeToRadian(270);

                    //setup chamber width
                    if (dbrProp.PropertyName == "Distance")
                      dbrProp.Value = PositionProperty.NumberOfPump * PositionProperty.DistanceOfPump + (double)30;
                    //text position for chamber
                    if (dbrProp.PropertyName == "Position X" && itemType == "chamber")
                      dbrProp.Value = (PositionProperty.NumberOfPump * PositionProperty.DistanceOfPump + (double)30) / 2;
                  }
                }
              }
            }
          }
          X += distance;
        }
        currentSpaceId.UpdateAnonymousBlocks();
      });
    }
  }  
}

