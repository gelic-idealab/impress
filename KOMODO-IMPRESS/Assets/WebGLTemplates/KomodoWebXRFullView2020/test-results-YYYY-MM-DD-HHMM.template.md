# Impress WebXR Client Test YYYY-MM-DD-HHMM

Impress
YYYY-MM-DD-HHMM

```
# Template
## [Section]
**[Subsection]**
\[Test\] [RESULT]

RESULT = …

- P.ASS 
- F.AIL / NETF.AIL
- D.EFER / NETD.EFER
- W.ORKAROUND / NETW.ORKAROUND
- N./T 
```

# VR Tests

- Person      _NAME_
- Device      _DEVICE_
- VR Type     _STANDALONE_OR_TETHERED_
- Browser     _BROWSER_

_COPY_FOR_EACH_PERSON_

## Client Manager 

**Syncs Poses**
- Head _RESULT_
- Left Hand _RESULT_
- Right Hand _RESULT_
- Hand animation state _RESULT_
- Performs Well _RESULT_
- Client Count Limit >= _NUMBER_
- Latency <= _SECONDS_

## Interactions and Tools

**VR menu** 
- Only shows spectator content initially _RESULT_
- Only one panel shows at a time _RESULT_
- Menu is comfortably placed for both hands DEFER
- Only current tab is highlighted _RESULT_
- On enter VR, shows Create panel _RESULT_
- On enter VR, shows height calibration _RESULT_
- Activate / Deactivate _RESULT_
- Handedness / cursor _RESULT_
- Hover Cursor _RESULT_
- Menu loads correctly on every entry _RESULT_
- Left / Right switch hands works well _RESULT_

**Can world pull**
- can translate with correct offset _RESULT_
- can rotate with correct offset _RESULT_
- can scale with correct offset _RESULT_
  - scale after height adjustment _RESULT_
  - scale after height calibration _RESULT_
- snap turns with left and right hands _RESULT_
- snap turn offset is correct _RESULT_

**Can use draw tool**
- draw tool does not open immediately _RESULT_
- hover color _RESULT_
- select color _RESULT_
- compare color _RESULT_
- draw with left hand _RESULT_
- draw with right hand _RESULT_
- pauses during world pulling DEFER
- strokes have correct color and z depth re:models, avatars _RESULT_
- strokes have correct z depth re:each other DEFER
- strokes have correct z depth while drawing _RESULT_
- strokes have correct z depth after drawing is done _RESULT_
- stroke size changes when world pull scaling _RESULT_
- can grab strokes _RESULT_
- 

**erase**
- can activate eraser _RESULT_
- can deactivate eraser _RESULT_
- with left hand _RESULT_
- with right hand _RESULT_
- erase others' stuff _RESULT_

**undo/redo**
- undo draw _RESULT_
- undo erase _RESULT_

**Can use settings** 
- height adjustment _RESULT_

**Starting position**
- goes to starting location _RESULT_
- height restores _RESULT_

**Can use people**
- client list _RESULT_
- client names above head _RESULT_

## Models

**Run-time import**
- Loads Performant _RESULT_

**Model Interactions**
- Can Grab model _RESULT_
- Grab model pack _RESULT_
- Grab synchronizes _RESULT_
- Grabbing locks for others _RESULT_
- Can Scale model _RESULT_
- Scale model pack _RESULT_
- Scale synchronizes _RESULT_
- Scaling locks for others _RESULT_
- Can Show / Hide _RESULT_
- Synced show / hide _RESULT_
- Synced show / hide for model packs _RESULT_
- Can lock model _RESULT_
- Can lock model pack _RESULT_
- Grab lock vs UI lock _RESULT_

**Performs Well** 
- Texture Limit _RESULT_
- Vertex Limit _RESULT_
- Count Limit _RESULT_

## Scene

- Light Limit _RESULT_
- Texture Limit _RESULT_
- Vertex Limit _RESULT_
- Count Limit _RESULT_

## Connection
- Avatars disappear upon leaving or disconnecting _RESULT_

**Catches Up State Upon (Re-)Entry**
- Auto connect _RESULT_
- Get Joined to Session _RESULT_
- Avatar names _RESULT_
- Avatar people _RESULT_
- Avatar poses _RESULT_
- Model poses _RESULT_
- Model pack poses _RESULT_
- visibility for models _RESULT_
- visibility for model packs _RESULT_
- lock for models _RESULT_
- lock for model packs _RESULT_
- scale for models _RESULT_
- scale for model packs _RESULT_
- “You just joined” own DEFER
- “Someone just joined” others DEFER
- “You just left” own _RESULT_
- “Someone just left” others _RESULT_
- Close Connection and Rejoin simulated offline _RESULT_
- Drawing poses, color _RESULT_

# Spectator Tests

- Person      _PERSON_
- Device      _PC_OR_MAC_
- Browser     _BROWSER_AND_VERSION_NUMBER_

## Testing WebXR Client

**General** 
- Renders Correctly _RESULT_

**Sync poses**
- Head _RESULT_
- Can Show / Hide _RESULT_
- Synced show / hide _RESULT_
- Can Lock _RESULT_
- Synced single-object lock _RESULT_
- Synced multi-object lock _RESULT_
- Lock/Unlock Show/Hide syncs on user click only _RESULT_

**Lighting**
- Models are unaffected by environment lighting _RESULT_
- Drawings are unaffected by environment lighting _RESULT_

**Network connection**
- Display session name _RESULT_
- Display runtime app and build _RESULT_
- Display server name _RESULT_
- Display session number _RESULT_S
- Connect to sync name space _RESULT_
- Display Sync ID _RESULT_
- Display ping/pong _RESULT_
- Display client names _RESULT_
- No extra clients _RESULT_
- Close connection and rejoin DEFER
- Leave and rejoin DEFER

**Spectator-only menu**
- Height calibration and up/down are hidden for spectator mode _RESULT_
- Create panel is hidden for spectator mode _RESULT_

**Can use keyboard controls** 
- rotate (`Q` `E` `2` `3` or Left Mouse Drag) _RESULT_
- strafe (`W` `A` `S` `D`) _RESULT_
- pan (Middle Mouse Drag) DEFER
- hyperspeed scroll (Mouse Scroll) _RESULT_

## Bugs
- _STEPS_TO_REPRODUCE_ [_FOOTNOTE_NUMBER_]
  - Expected: _EXPECTED_RESULT_
  - Actual: _ACTUAL_RESULT_

Additional Notes
- _NOTE_

## Filed Issues

1. [_ISSUE_NAME_](_ISSUE_URL_)