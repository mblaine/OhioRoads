@speed-5: rgba(105, 0, 143, 1);
@speed-10: rgba(12, 179, 167, 1);
@speed-20: blue;
@speed-30: rgba(2, 158, 2, 1);
@speed-40: #f5f436;
@speed-50: rgba(201, 151, 22, 1);
@speed-60: red;
@speed-70: magenta;

@surface-none: rgba(245, 245, 245, 1);
@surface-concrete: rgba(170, 170, 170, 1);
@surface-chipseal: #595757;
@surface-impass: rgba(125, 24, 12, 1);
@surface-unpaved: #73603c;
@surface-gravel: rgba(133, 134, 133, 1);
@surface-bricks: rgba(180, 46, 62, 1);
@surface-asphalt: rgba(51, 51, 51, 1);

@text-color-main: white;
@text-halo-main: black;

@text-color-second: white;
@text-halo-second: #474747;

@text-color-third: rgba(53, 52, 52, 1);
@text-halo-third: rgba(226, 224, 224, 1);

@text-color-fourth: white;
@text-halo-fourth: #695c37;


@text-color-alt: rgba(0, 0, 0, 1);
@text-halo-alt: rgba(205, 243, 70, 1);

@casing-width-lte8: 1;
@casing-width-9: 1;
@casing-width-10: 1;
@casing-width-11: 1;
@casing-width-12: 4;
@casing-width-13: 8.53;
@casing-width-14: 13.06;
@casing-width-15: 17.6;
@casing-width-16: 33.2;
@casing-width-gte17: 40;

@road-width-lte11: 0;
@road-width-12: 1;
@road-width-13: 3.5;
@road-width-14: 7;
@road-width-15: 10;
@road-width-16: 22.5;
@road-width-gte17: 30;


#roads-casing[maxspeed!=null]
{
	line-join: round;
	line-cap: round;
	[zoom<=8] { line-width: @casing-width-lte8; }
	[zoom=9] { line-width: @casing-width-9; }
	[zoom=10] { line-width: @casing-width-10; }
	[zoom=11] { line-width: @casing-width-11; }
	[zoom=12] { line-width: @casing-width-12; }
	[zoom=13] { line-width: @casing-width-13; }
	[zoom=14] { line-width: @casing-width-14; }
	[zoom=15] { line-width: @casing-width-15; }
	[zoom=16] { line-width: @casing-width-16; }
	[zoom>=17] { line-width: @casing-width-gte17; }
	
	[zoom<=11][maxspeed>25] {line-width: @casing-width-11 * 2}
	
	[maxspeed>=5] { line-color: @speed-5; }
	[maxspeed>=10] { line-color: @speed-10; }
	[maxspeed>=20] { line-color: @speed-20; }
	[maxspeed>=30] { line-color: @speed-30; }
	[maxspeed>=40] { line-color: @speed-40; }
	[maxspeed>=50] { line-color: @speed-50; }
	[maxspeed>=60] { line-color: @speed-60; }
	[maxspeed>=70] { line-color: @speed-70; }
	
}

#roads-casing[maxspeed!=null]::hatch
{
	line-opacity: 0;
	[maxspeed=10],[maxspeed=30],[maxspeed=40],[maxspeed=50],[maxspeed=70]
	{
		line-pattern-file: url("sprites/hatch.png");
		line-pattern-type: warp;
		line-pattern-alignment: global;
		line-pattern-opacity: 0.5;
		[zoom<=8] { line-pattern-width: @casing-width-lte8; line-pattern-file: url("sprites/hatch-11.png"); }
		[zoom=9] { line-pattern-width: @casing-width-9; line-pattern-file: url("sprites/hatch-11.png"); }
		[zoom=10] { line-pattern-width: @casing-width-10; line-pattern-file: url("sprites/hatch-11.png"); }
		[zoom=11] { line-pattern-width: @casing-width-11; line-pattern-file: url("sprites/hatch-11.png"); }
		[zoom=12] { line-pattern-width: @casing-width-12; line-pattern-file: url("sprites/hatch-12.png"); }
		[zoom=13] { line-pattern-width: @casing-width-13; line-pattern-file: url("sprites/hatch-13.png"); }
		[zoom=14] { line-pattern-width: @casing-width-14; line-pattern-file: url("sprites/hatch-14.png"); }
		[zoom=15] { line-pattern-width: @casing-width-15; line-pattern-file: url("sprites/hatch-15.png"); }
		[zoom=16] { line-pattern-width: @casing-width-16; line-pattern-file: url("sprites/hatch-16.png"); }
		[zoom>=17] { line-pattern-width: @casing-width-gte17; }
	}
	[maxspeed=20],[maxspeed=60]
	{
		line-pattern-file: url("sprites/hatch-alt.png");
		line-pattern-type: warp;
		line-pattern-alignment: global;
		line-pattern-opacity: 0.5;
		[zoom<=8] { line-pattern-width: @casing-width-lte8; line-pattern-file: url("sprites/hatch-alt-11.png"); }
		[zoom=9] { line-pattern-width: @casing-width-9; line-pattern-file: url("sprites/hatch-alt-11.png"); }
		[zoom=10] { line-pattern-width: @casing-width-10; line-pattern-file: url("sprites/hatch-alt-11.png"); }
		[zoom=11] { line-pattern-width: @casing-width-11; line-pattern-file: url("sprites/hatch-alt-11.png"); }
		[zoom=12] { line-pattern-width: @casing-width-12; line-pattern-file: url("sprites/hatch-alt-12.png"); }
		[zoom=13] { line-pattern-width: @casing-width-13; line-pattern-file: url("sprites/hatch-alt-13.png"); }
		[zoom=14] { line-pattern-width: @casing-width-14; line-pattern-file: url("sprites/hatch-alt-14.png"); }
		[zoom=15] { line-pattern-width: @casing-width-15; line-pattern-file: url("sprites/hatch-alt-15.png"); }
		[zoom=16] { line-pattern-width: @casing-width-16; line-pattern-file: url("sprites/hatch-alt-16.png"); }
		[zoom>=17] { line-pattern-width: @casing-width-gte17; }
	}
}

#roads-casing[maxspeed=null][zoom<=11]
{
	line-join: round;
	line-cap: round;
	line-width: 1;
	line-color: rgb(155, 155, 155);
}

#roads-fill
{
	line-join: round;
	line-cap: round;
	line-color: @surface-none;
	[zoom<=11] { line-width: @road-width-lte11; }
	[zoom=12] { line-width: @road-width-12; }
	[zoom=13] { line-width: @road-width-13; }
	[zoom=14] { line-width: @road-width-14; }
	[zoom=15] { line-width: @road-width-15; }
	[zoom=16] { line-width: @road-width-16; }
	[zoom>=17] { line-width: @road-width-gte17; }
	[surface="concrete"] { line-color: @surface-concrete; }
	[surface="chipseal"] { line-color: @surface-chipseal; }
	[surface="impass"] { line-color: @surface-impass; }
	[surface="unpaved"] { line-color: @surface-unpaved; }
	[surface="gravel"] { line-color: @surface-gravel; }
	[surface="bricks"] { line-color: @surface-bricks; }
	[surface="asphalt"] { line-color: @surface-asphalt; }
	
	[surface!=null][surface!="asphalt"]
	{
		line-pattern-join: round;
		line-pattern-cap: round;
		line-pattern-type: repeat;
        line-pattern-alignment: global;
        line-pattern-file: url("sprites/none.png");
		[zoom<=11] { line-pattern-width: @road-width-lte11; }
		[zoom=12] { line-pattern-width: @road-width-12; }
		[zoom=13] { line-pattern-width: @road-width-13; }
		[zoom=14] { line-pattern-width: @road-width-14; }
		[zoom=15] { line-pattern-width: @road-width-15; }
		[zoom=16] { line-pattern-width: @road-width-16; }
		[zoom>=17] { line-pattern-width: @road-width-gte17; }
		[surface="concrete"] {line-pattern-file: url("sprites/concrete.png"); }
		[surface="chipseal"] {line-pattern-file: url("sprites/chipseal.png"); }
		[surface="impass"] {line-pattern-file: url("sprites/impassible.png"); }
		[surface="unpaved"] {line-pattern-file: url("sprites/unpaved.png"); }
		[surface="gravel"] {line-pattern-file: url("sprites/gravel.png"); }
		[surface="bricks"] {line-pattern-file: url("sprites/bricks.png"); }
	}
}

#roads-lanes[lanes>=2]
{
	line-opacity: 0;
	line-pattern-join: round;
	line-pattern-cap: round;
	line-pattern-file: url("sprites/none.png");
	line-pattern-type: warp;
	line-pattern-alignment: global;
	/*[zoom<=11] { line-pattern-width: @road-width-lte11; }
	[zoom=12] { line-pattern-width: @road-width-12; }
	[zoom=13] { line-pattern-width: @road-width-13; }
	[zoom=14] { line-pattern-width: @road-width-14; }
	[zoom<=15] { line-pattern-width: @road-width-15; }*/
	[zoom<=16] { line-pattern-width: @road-width-16; }
	[zoom>=17] { line-pattern-width: @road-width-gte17; }
	[lanes=2] {line-pattern-file: url("sprites/lanes-2.png"); }
	[lanes=3] {line-pattern-file: url("sprites/lanes-3.png"); }
	[lanes=4] {line-pattern-file: url("sprites/lanes-4.png"); }
	[lanes=5] {line-pattern-file: url("sprites/lanes-5.png"); }
	[lanes=6] {line-pattern-file: url("sprites/lanes-6.png"); }
	[lanes=7] {line-pattern-file: url("sprites/lanes-7.png"); }
	[lanes=8] {line-pattern-file: url("sprites/lanes-8.png"); }
	[lanes=9] {line-pattern-file: url("sprites/lanes-9.png"); }
	[lanes=10] {line-pattern-file: url("sprites/lanes-10.png"); }
}


#roads-oneway[oneway!=null]
{
	line-opacity: 0;
	line-pattern-join: round;
	line-pattern-cap: round;
	line-pattern-file: url("sprites/forward.png");
	line-pattern-type: warp;
	line-pattern-alignment: global;
	/*[zoom<=11] { line-pattern-width: @road-width-lte11; }
	[zoom=12] { line-pattern-width: @road-width-12; }
	[zoom=13] { line-pattern-width: @road-width-13; }*/
	[zoom<=14] { line-pattern-width: @road-width-13; }
	[zoom=15] { line-pattern-width: @road-width-14; }
	[zoom=16] { line-pattern-width: @road-width-16; }
	[zoom>=17] { line-pattern-width: @road-width-gte17; }
	[oneway="-1"] {line-pattern-file: url("sprites/backward.png"); }
	[zoom<=16]
	{ 
		line-pattern-file: url("sprites/forward-small.png"); 
		[oneway="-1"] { line-pattern-file: url("sprites/backward-small.png"); }
	}
}

#roads-label-lanes[lanes!=null]
{
	shield-name: [lanes];
    shield-face-name: @main-fonts;
    shield-size: 11;
    shield-placement: line;
    shield-spacing: 100;
    shield-halo-radius: 1;
	shield-character-spacing: 1;
    shield-halo-fill: @text-halo-third;
	shield-fill: @text-color-third;
	shield-allow-overlap: true;
	shield-file: url("sprites/none.png");
}

#roads-label-width
{
	[width!=null], ["shoulder:left:width"!=null], ["shoulder:right:width"!=null]
	{
		text-name: [shoulder:left:width] + "'\n" + [width] + "'\n" + [shoulder:right:width] + "'";
		text-face-name: @main-fonts;
		text-size: 11;
		text-placement: line;
		text-spacing: 150;
		text-halo-radius: 3;
		text-character-spacing: 1;
		text-halo-fill: @text-halo-fourth;
		text-fill: @text-color-fourth;
		text-allow-overlap: false;
		text-align:right;
		text-vertical-alignment: middle;
		text-upright: right-only;
		text-dy:2;
	}	
}

#roads-label-width::otherway
{
	[width!=null], ["shoulder:left:width"!=null], ["shoulder:right:width"!=null]
	{
		text-name: [shoulder:right:width] + "'\n" + [width] + "'\n" + [shoulder:left:width] + "'";
		text-face-name: @main-fonts;
		text-size: 11;
		text-placement: line;
		text-spacing: 150;
		text-halo-radius: 3;
		text-character-spacing: 1;
		text-halo-fill: @text-halo-fourth;
		text-fill: @text-color-fourth;
		text-allow-overlap: false;
		text-align:right;
		text-vertical-alignment: middle;
		text-upright: left-only;
		text-dy:-2;
	}	
}


#roads-label-other[SURFACE_TYPE_CD!=null]
{
	text-name: [surface] + '(' + [SURFACE_TYPE_CD] + ')';
    text-face-name: @main-fonts;
    text-size: 10;
    text-placement: line;
    text-spacing: 275;
    text-halo-radius: 2;
	text-character-spacing: 1;
    text-halo-fill: @text-halo-second;
	text-fill: @text-color-second;
	text-allow-overlap: false;
	text-dy: 14;
}

#roads-label-other[maxspeed!=null]::other2
{
	text-name: [maxspeed] + ' mph';
    text-face-name: @main-fonts;
    text-size: 10;
    text-placement: line;
    text-spacing: 275;
    text-halo-radius: 2;
	text-character-spacing: 1;
    text-halo-fill: @text-halo-second;
	text-fill: @text-color-second;
	text-allow-overlap: false;
	text-dy: -14;
}

#roads-text-name
{
	text-name: [name];
    text-face-name: @main-fonts;
    text-size: 13;
	[zoom<=16] { text-size: 11; }
    text-placement: line;
    text-spacing: 300;
    text-halo-radius: 2;
	text-character-spacing: 1;
    text-halo-fill: @text-halo-main;
	text-fill: @text-color-main;
	text-vertical-alignment: middle;
	text-allow-overlap: true;
	[zoom=16] { text-name: [name] + '\n' + SIZE9[ref]; }
	[zoom>=17] { text-name: [name] + '\n' + SIZE11[ref]; }
	[width!=null], ["shoulder:left:width"!=null], ["shoulder:right:width"!=null]
	{
		text-halo-fill: @text-halo-alt;
		text-fill: @text-color-alt;
	}
	[zoom>=18]
	{
		text-placement-type: list;
	}
}