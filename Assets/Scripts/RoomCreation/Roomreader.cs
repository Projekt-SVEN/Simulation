//#define DEBUG_FILEREADER

using Assets.Scripts.RoomCreation;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;

namespace Assets.Scripts
{
    class RoomReader
    {
        private const string PATHWINDOW = "/Roomplan/Furniture/Window";
        private const string PATHDOOR = "/Roomplan/Furniture/Door";
        private const string PATHCHAIR = "/Roomplan/Furniture/Chair";
        private const string PATHTABLE = "/Roomplan/Furniture/Table";
        private const string PATHHEATER = "/Roomplan/Furniture/Heater";
        private const string PATHCLOSET = "/Roomplan/Furniture/Closet";
        private const string PATHTABLET = "/Roomplan/Furniture/Tablet";
        private const string PATHTHERMOMETER = "/Roomplan/Furniture/Thermometer";

        private const string PATHROOMHEIGTH = "/Roomplan/Room/Height";
        private const string PATHROOMWIDTH = "/Roomplan/Room/Width";
        private const string PATHROOMWALLTHICKNESS = "/Roomplan/Room/WallThickness";
        private const string PATHROOMTHERMALPIXELSIZE = "/Roomplan/Room/ThermalPixelSize";
        private const string PATHROOMWALLVERTEXDISTANCE = "/Roomplan/Room/WallVertexDisatnce";
        private const string PATHROOMVERTEXOBJECTOFFSET = "/Roomplan/Room/VertexObjectOffSet";

        private const string PATHHEIGHT = "./Position/Height";
        private const string PATHWIDTH = "./Position/Width";
        private const string PATHROTATION= "./Rotation";
        private const string PATHSIZEHEIGHT= "./Size/Height";
        private const string PATHSIZEWIDTH= "./Size/Width";
        private const string PATHTYPE= "./Type";
        private const string PATHGETFEHMNAME = "./FHEM-Name-Get";
        private const string PATHSETFEHMNAME = "./FHEM-Name-Set";

        private string _xmlPath;

        public RoomReader(string xmlPath)
        {
            _xmlPath = xmlPath;
        }

        public RoomObject[] ReadRoom()
        {

            #if DEBUG_FILEREADER
                Debug.Log("Enter");
            #endif

            //StreamReader str = new StreamReader(_xmlPath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_xmlPath);

            XmlNode xmlRootNode = xmlDoc.DocumentElement;

            #if DEBUG_FILEREADER
                Debug.Log("Load File");
            #endif

            #region Room Options
            {
                //read Roomsize
                XmlNode heightNode = xmlRootNode.SelectSingleNode(PATHROOMHEIGTH);
                OptionsManager.RoomHeight = int.Parse(heightNode.InnerText);

                XmlNode widthNode = xmlRootNode.SelectSingleNode(PATHROOMWIDTH);
                OptionsManager.RoomWidth = int.Parse(widthNode.InnerText);

                XmlNode wallThickness = xmlRootNode.SelectSingleNode(PATHROOMWALLTHICKNESS);
                
                //CultureInfo.InvariantCulture wird benötigt, damit Zahlen, wie 0.5, nicht als 5 gelesen werden!
                OptionsManager.WallThickness = float.Parse(wallThickness.InnerText, CultureInfo.InvariantCulture);

                XmlNode thermalPixelSize = xmlRootNode.SelectSingleNode(PATHROOMTHERMALPIXELSIZE);
                OptionsManager.ThermalPixelSize = float.Parse(thermalPixelSize.InnerText, CultureInfo.InvariantCulture);

                XmlNode vertexObjectOffSet = xmlRootNode.SelectSingleNode(PATHROOMVERTEXOBJECTOFFSET);
                OptionsManager.VertexObjectOffSet = float.Parse(vertexObjectOffSet.InnerText, CultureInfo.InvariantCulture);

                XmlNode wallVertexDistance = xmlRootNode.SelectSingleNode(PATHROOMWALLVERTEXDISTANCE);
                OptionsManager.WallVertexDistance = float.Parse(wallVertexDistance.InnerText, CultureInfo.InvariantCulture);
            }
            #endregion

            List<RoomObject> roomObjects = new List<RoomObject>();

            #region Windows and Doors

            XmlNodeList windows = xmlRootNode.SelectNodes(PATHWINDOW);
            roomObjects = readRoomobjects(roomObjects, windows, RoomObject.RoomElement.Window);

            XmlNodeList doors = xmlRootNode.SelectNodes(PATHDOOR);
            roomObjects = readRoomobjects(roomObjects, doors, RoomObject.RoomElement.Door);

            #endregion

            #region furniture

            XmlNodeList chairs = xmlRootNode.SelectNodes(PATHCHAIR);
            roomObjects = readRoomobjects(roomObjects, chairs, RoomObject.RoomElement.Chair);

            XmlNodeList tables = xmlRootNode.SelectNodes(PATHTABLE);
            roomObjects = readRoomobjects(roomObjects, tables, RoomObject.RoomElement.Table);

            XmlNodeList heater = xmlRootNode.SelectNodes(PATHHEATER);
            roomObjects = readRoomobjects(roomObjects, heater, RoomObject.RoomElement.Heater);

            XmlNodeList closet = xmlRootNode.SelectNodes(PATHCLOSET);
            roomObjects = readRoomobjects(roomObjects, closet, RoomObject.RoomElement.Closet);

            XmlNodeList tablet = xmlRootNode.SelectNodes(PATHTABLET);
            roomObjects = readRoomobjects(roomObjects, tablet, RoomObject.RoomElement.Tablet);

            XmlNodeList thermometer = xmlRootNode.SelectNodes(PATHTHERMOMETER);
            roomObjects = readRoomobjects(roomObjects, thermometer, RoomObject.RoomElement.Thermometer);

            #endregion

            return roomObjects.Count == 0 ? new RoomObject[0] : roomObjects.ToArray();

        }

        
        private List<RoomObject> readRoomobjects(List<RoomObject> roomObjects, XmlNodeList elementList, RoomObject.RoomElement type)
        {
            if(elementList != null)
            {
                foreach(XmlNode element in elementList)
                {
                    RoomObject newRoomObject = new RoomObject();
                    newRoomObject.Element = type;

                    XmlNode pos = element.SelectSingleNode(PATHHEIGHT);
                    newRoomObject.PositionY = float.Parse(pos.InnerText, CultureInfo.InvariantCulture);

                    pos = element.SelectSingleNode(PATHWIDTH);
                    newRoomObject.PositionX = float.Parse(pos.InnerText, CultureInfo.InvariantCulture);

                    pos = element.SelectSingleNode(PATHROTATION);
                    newRoomObject.Rotation = float.Parse(pos.InnerText);

                    pos = element.SelectSingleNode(PATHTYPE);
                    newRoomObject.Type = pos.InnerText;

                    pos = element.SelectSingleNode(PATHGETFEHMNAME);
                    newRoomObject.FhemGetName = pos.InnerText;

                    pos = element.SelectSingleNode(PATHSETFEHMNAME);
                    newRoomObject.FhemSetName = pos.InnerText;

                    pos = element.SelectSingleNode(PATHSIZEHEIGHT);
                    newRoomObject.SizeHeight = float.Parse(pos.InnerText, CultureInfo.InvariantCulture);

                    pos = element.SelectSingleNode(PATHSIZEWIDTH);
                    newRoomObject.SizeWidth = float.Parse(pos.InnerText, CultureInfo.InvariantCulture);
                    
                    roomObjects.Add(newRoomObject);
                }
            }

            return roomObjects;
        }
    }
}
