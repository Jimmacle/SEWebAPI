<?
$server = "sewa.mine.nu";
$secretkey = $_REQUEST['secretkey'];
$self = $_SERVER['PHP_SELF'];

$myfile = fopen("gridUrls.conf", "r");
$confFile = fread($myfile,filesize("gridUrls.conf"));

## steam connect line for this server up top
print "<a href=\"steam://connect/$server:27016\">Connect to this server via Steam</a> <br />\n";

$options = array(
  'http' => array(
    'method'  => 'GET',
    'header'=>  "Content-Type: application/json\r\n" .
    "Accept: application/json\r\n"
  )
);

$url = "http://".$server."/".$secretkey."/grid";
$context  = stream_context_create( $options );
$result = file_get_contents( $url, false, $context );
$grid = json_decode( $result );

print "<table> \n";  ## open big wrapping table
print "<tr><td valign=\"top\"> \n"; ## left column in the big wrapping table

print "<h2> Grid name: ". $grid->name. "</h2>\n";
print "Block Count: ". $grid->blocks. "<br/>\n";
print "Grid <br/> <ul><li>x:". $grid->pos->x."</li> <li>y:". $grid->pos->y."</li> <li>z:".$grid->pos->z. "</li></ul>\n";
print "Rotation <ul> <li>x:". $grid->rot->x.
      "</li> <li>y:".$grid->rot->y.
      "</li> <li>z:".$grid->rot->z.
      "</li> <li>w:".$grid->rot->w.
      "</li> </ul>\n";

print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks\">List All Blocks</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=cargo\">List All Cargo</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=door\">List All Doors</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=Reactor\">List All Reactor</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=batteryblock\">List All Battery</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=MyRefinery\">List All Refinery</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=MyAssembler\">List All Assembler</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=MyThrust\">List All Thruster</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=MyShipMergeBlock\">List All Merge Blocks</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=MyShipConnector\">List All Connectors</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=Light\">List All Lights</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=Oxygen\">List All Oxygen Generators</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=Tank\">List All Tanks</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=Antenna\">List All Antennas</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=Gyro\">List All Gyros</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=program\">List All Programmable Block</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=MyGravityGenerator\">List All Gravity Generator</a><br />\n";
print "<a href=\"".$self."?secretkey=".$secretkey."&request=blocks&type=MyAirVent\">List All Airvents</a><br />\n";

print "<br/> <br/>\n";
print "Grid Notifications <br/>\n";
print "<table border=\"1\">\n";
print "<tr><td> <font color=\"#33ff33\">[O] </font></td> <td> Raw Ore <a href=\"#\" title=\"Details on this notofication are here\">[i]</a> </td> </tr>\n";  ## TODO, a click should take me to edit notify
print "<tr><td> <font color=\"#33ff33\">[O] </font></td> <td> Ur Ingots </td> </tr>\n";
print "<tr><td> <font color=\"#ff3333\">[X] </font></td> <td> Bullets Fired?</td> </tr>\n";
print "<tr><td> <font color=\"#ff9933\">[!] </font></td> <td> Bullets Low?</td> </tr>\n";
print "<tr><td colspan=\"2\"> <a href=\"#\">Create New Notification ...</a></td> </tr>\n";
print "</table>\n";

print "</td><td valign=\"top\">\n"; ## close the left bar on the big wrapping table, and open the right one

## show all blocks in grid
if ( $_REQUEST['request'] == "blocks" && $_REQUEST['secretkey'] != "" && $_REQUEST['blockID'] == "" ) {
  print "<h2>BLOCKS </h2>\n";
  if ($_REQUEST['type'] != '' ) {
    $typeSearch = "?type=".$_REQUEST['type'];
  } else {
    $typeSearch = "";
  }
  $urlBlocks = "http://".$server."/".$secretkey."/blocks".$typeSearch;
  $options = array(
    'http' => array(
      'method'  => 'GET',
      'header'=>  "Content-Type: application/json\r\n" .
      "Accept: application/json\r\n"
    )
  );
  $context  = stream_context_create( $options );
  $result = file_get_contents( $urlBlocks, false, $context );
  $blocks = json_decode( $result );

  #   Itterate over list of blocks with toggle for each block true false, doing a json post of true/false, updating page back to this block
  print "<table border=\"1\">\n";
  foreach ($blocks as $block) {
    displayBlock ( $block, $self, $secretkey );
  }
  print "<tr></tr>\n";
  print "<tr><td> <input type=\"checkbox\" name=\"checkAll\" value=\"All\"> </td><td colspan=\"2\" align=right><input type=\"submit\" value=\"Toggle: OnOff\"> - <input type=\"submit\" value=\"Toggle: ShowOnHud\"></td></tr>\n";
  print "<tr><td colspan=\"3\" align=right><input type=\"submit\" value=\"Toggle: OpenClose\"> - <input type=\"submit\" value=\"Toggle: Use Conveyor\"></td></tr>\n";
  print "</table>\n";
}

## process true/false toggle
if ( $_REQUEST['secretkey'] != "" && $_REQUEST['blockID'] != "" && $_REQUEST['request'] == "toggle" && $_REQUEST['propertyValue'] != "" ) {

  if ( $_REQUEST['propertyValue'] == "False" ) { $newToggle = "True"; } else { $newToggle = "False"; }
  # print "toggleing FROM ". $_REQUEST['propertyValue'] . " to ". $newToggle . " for block ID ". $_REQUEST['blockID'];

  $data = array(
    'data' => array(
      array(
        'id' => $_REQUEST['blockID'],
        'property' =>  $_REQUEST['propertyName'],
        'value' => $newToggle
      )
    )
  );

  print json_encode ($data);

  $toggleUrl = "http://".$server."/".$secretkey."/blocks";
  $options = array(
    'http' => array(
      'method'  => 'PUT',
      'content' => json_encode( $data ),
      'header'=>  "Content-Type: application/json\r\n" .
      "Accept: application/json\r\n"
    )
  );
  $context  = stream_context_create( $options );
  $result = file_get_contents( $toggleUrl, false, $context );
  $response = json_decode( $result );

  print_r ($response); # TODO

}

## display just block ID provided
if ( $_REQUEST['secretkey'] != "" && $_REQUEST['blockID'] != "" ) {
  # print "displaying just blockID ". $_REQUEST['blockID'] ."<br />\n";
  $blockUrl = "http://".$server."/".$secretkey."/blocks/".$_REQUEST['blockID'];

  $options = array(
    'http' => array(
      'method'  => 'GET',
      'header'=>  "Content-Type: application/json\r\n" .
      "Accept: application/json\r\n"
    )
  );
  $context  = stream_context_create( $options );
  $result = file_get_contents( $blockUrl, false, $context );
  $block = json_decode( $result );

  print "<table border=\"1\">\n";
  displayBlock ( $block, $self, $secretkey );

  print "</table>\n";

}

if ( $_REQUEST['secretkey'] != "" && $_REQUEST['blockID'] != "" && $_REQUEST['request'] == 'inventory' ) {
  # print "displaying inventory of block ". $_REQUEST['blockID'];
  $blockInventoryUrl =  "http://".$server."/".$secretkey."/blocks/".$_REQUEST['blockID']. "/inventory";
  $options = array(
    'http' => array(
      'method'  => 'GET',
      'header'=>  "Content-Type: application/json\r\n" .
      "Accept: application/json\r\n"
    )
  );
  $context  = stream_context_create( $options );
  $result = file_get_contents( $blockInventoryUrl, false, $context );
  $contents = json_decode( $result );
  displayContents ( $contents );

}
print "</td></tr> </table>\n";  ## close big wrapping table

function displayBlock ( $block, $self, $secretkey ) {

  $containerTypes = array( "MyAssembler", "MyRefinery", "MyShipConnector", "MyLargeGatlingTurret", "MyCargoContainer", "MyShipDrill", "MyReactor", "MyGasTank", "MyCockpit", "MyGasGenerator", "MyCryoChamber", "MyLargeGatlingTurret", "MyLargeInteriorTurret" );

  # print "<h3>". $block->name."</h3>";
  print "<tr>\n";
  print "<td> <input type=\"checkbox\" name=\"checked\" value=\"".$block->id."\"></td>\n";
  print "<td><h3>".$block->name."</h3></td><td></td></tr>\n";

  foreach ($block->properties as $propertyName => $propertyValue ) {
    if ( $propertyValue == "True" || $propertyValue == "False" ) {
      $toggle = "<a href=\"".$self."?secretkey=".$secretkey."&request=toggle&blockID=".$block->id."&propertyName=".$propertyName."&propertyValue=".$propertyValue."\">toggle</a>\n";
    } else {
      $toggle = "";
    }
    print "<tr><td></td><td>".$propertyName. " = ". $propertyValue . "</td><td>$toggle </td></tr>\n";
  }

  #  Link on each block's ID to see cargo http://sewa.mine.nu/4Cl0xlirgcOOJ7SU/blocks/83572021464198810/inventory
  if ( in_array($block->type, $containerTypes) ) {
    $contentLink = "<a href=\"".$self."?secretkey=".$secretkey."&blockID=".$block->id."&request=inventory\">contents</a><br />\n";
  } else {
    $contentLink = "";
  }
  print "<tr><td colspan=3>".$contentLink."</td></tr>\n";
  # print "<br />\n";

}

function displayContents ( $contents ) {
  # print "<pre>";
  # print_r ($contents); # TODO
  # print "</pre>";
  print "<h4>Inventory</h4>";
  
  # print "<ul>\n";
  print "<form action=\"".$self."\">\n";
  print "<table border=\"1\">\n";
  # print "<tr><td><b>Item Name</b></td> <td><b>Amount</b></td> <td><sub>min</sub> <em>/</em> <sup>max</sup><br /></td><td></td>\n";
  print "<tr><td><b>Item Name</b></td> <td><b>Amount</b></td> <td></td>\n";
  foreach ($contents->data as $item ) {
    # print "<li>". $item->name . " -- ".$item->amount . " </li>\n";
    # print "<tr><td> <input type=\"text\" name=\"item.name\" value=\"". $item->name . "\"> </td> <td>".$item->amount . " </td> </tr>\n";
    # print "<tr><td> ".$item->name. "</td> <td>".$item->amount . "</td> <td><sub>10</sub> <em>/</em> <sup>10,000</sup> </td> <td> <input type=\"checkbox\" name=\"checked\" value=\"".$item->id."\"></tr>\n";
    print "<tr><td> ".$item->name. "</td> <td>".$item->amount . "</td> <td> <input type=\"checkbox\" name=\"checked\" value=\"".$item->id."\"></tr>\n";
  }
  print "<tr><td colspan=\"4\" align=right><input type=\"submit\" value=\"Merge All\"> - <input type=\"submit\" value=\"Split Checked\"></td></tr>\n";

  print "<tr><td colspan=\"4\" align=\"right\">";
  print "<select name=\"TargetBlock\">\n";
  print "<option value=\"Cargo Container 1\">Cargo Container 1</option>\n";
  print "<option value=\"Large Cargo Container 4\">Large Cargo Container 4</option>\n";

  print "<input type=\"submit\" value=\"Move Checked\"> - ";
  print "<input type=\"submit\" value=\"Split Checked\">";
  print "</td></tr>\n";

  print "<tr><td colspan=\"4\" align=\"right\">\n";
  print "<input type=\"text\" name=\"pullValue\" value=\"100\"> Pull: <input type=\"text\" name=\"pullName\" value=\"Steel Plate\"> <input type=\"submit\" value=\"Pull\">\n";
  print "</td></tr>\n";

  # print "<tr><td colspan=\"4\" align=\"right\">\n";
  # print "<input type=\"text\" name=\"notifyValue\" value=\"10\"> notify if: \n";
  # print "<select name=\"notifyType\">\n";
  # print "<option value=\"Under Value\">Under Value</option>\n";
  # print "<option value=\"Over Value\">Over Value</option>\n";
  # print "<input type=\"submit\" value=\"Notify\">\n";
  # print "</td></tr>\n";

  print "</table>\n";
  # print "</ul>\n";
}

?>
