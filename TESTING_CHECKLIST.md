# Testing Checklist for Refactored Save System

## Before Testing

### 1. Add GameSystemInitializer to Your Scene
- [ ] Open your main menu scene
- [ ] Create empty GameObject named "GameSystemInitializer"
- [ ] Add `GameSystemInitializer` component to it
- [ ] Save scene

### 2. Clear Old Data (Important!)
```
Delete old PlayerPrefs to start fresh:
1. In Unity Editor: Edit → Clear All PlayerPrefs (if available)
2. Or manually delete: 
   - Windows: Registry Editor → HKEY_CURRENT_USER\Software\[Company]\[Game]
   - Mac: ~/Library/Preferences/[Company].[Game].plist
   - Or use PlayerPrefs.DeleteAll() in code
```

## Testing Steps

### Phase 1: Check Runtime Managers Initialize

1. **Enter Play Mode**
   - [ ] Check Console for: `[GameSystemInitializer] Currency manager initialized`
   - [ ] Check Console for: `[GameSystemInitializer] Level progress manager initialized`
   - [ ] If not seen, the managers aren't initializing!

2. **Check Singleton Creation**
   - [ ] In Hierarchy, look for `GameCurrencyManager` GameObject
   - [ ] In Hierarchy, look for `LevelProgressRuntimeManager` GameObject
   - [ ] These should be DontDestroyOnLoad

### Phase 2: Test Gold System

3. **Add Gold**
   - [ ] Play level and earn gold
   - [ ] Check Console: `Added X gold from...`
   - [ ] Check Console: `[GameCurrencyManager] Gold saved to PlayerPrefs`

4. **Exit and Reload**
   - [ ] Exit Play Mode
   - [ ] Enter Play Mode again
   - [ ] Check Console: `[GameCurrencyManager] Gold loaded from PlayerPrefs: X`
   - [ ] Verify gold amount is correct in UI

### Phase 3: Test Level Progress

5. **Complete a Level**
   - [ ] Play Level 1
   - [ ] Win with stars
   - [ ] Check Console: `Level 1 completed: X points, Y stars`
   - [ ] Check Console: `[LevelProgressRuntimeManager] Progress saved to PlayerPrefs`

6. **Check UI Updates**
   - [ ] Return to level select
   - [ ] Check Console: `[LevelManager] Level X refresh: shouldBeUnlocked`
   - [ ] Check Console: `Level X: Displaying Y stars from runtime manager`
   - [ ] **Verify stars display on level button**

7. **Click Level Again**
   - [ ] Click completed level
   - [ ] PlayPopup should open
   - [ ] Check Console: `[PlayPopup] ConfigureLevel for level X`
   - [ ] Check Console: `[PlayPopup] Level X - BestScore: XXX, Stars: Y`
   - [ ] **Verify stars and score show in popup**

### Phase 4: Test Difficulty Switching

8. **Switch Difficulty**
   - [ ] Complete Level 1 on Easy
   - [ ] Switch to Normal difficulty
   - [ ] Check Console: `Progress reloaded and UI refreshed for difficulty: Basic`
   - [ ] **Verify Level 1 is locked again on Normal**
   - [ ] Switch back to Easy
   - [ ] **Verify Level 1 shows stars on Easy**

### Phase 5: Test Persistence

9. **Exit and Reload**
   - [ ] Complete levels and earn gold
   - [ ] Exit Play Mode
   - [ ] Enter Play Mode
   - [ ] **Verify all progress restored**

10. **Build and Test on Device (CRITICAL!)**
    - [ ] Build to Android/iOS
    - [ ] Play and earn progress
    - [ ] **Close app completely** (swipe away from recent apps)
    - [ ] Reopen app
    - [ ] **Verify all progress persisted!**

## Common Issues and Solutions

### Issue: Managers Not Initializing
**Symptoms:** No console logs about managers
**Solution:** 
- Make sure `GameSystemInitializer` is in the scene
- Check it's active and enabled
- Try manually calling `GameCurrencyManager.Instance` in Start()

### Issue: Stars Not Displaying
**Symptoms:** `[PlayPopup] LevelProgress retrieved: False`
**Solution:**
- Check if `LevelProgressRuntimeManager.Instance` exists
- Check Console for: `[LevelProgressRuntimeManager] Progress loaded`
- Verify difficulty is correct in logs

### Issue: Progress Not Saving
**Symptoms:** No "saved to PlayerPrefs" logs
**Solution:**
- Check `Initialized` is true in runtime manager
- Check `OnApplicationPause` is being called (test by backgrounding app)
- Manually call `.ForceSave()` to test

### Issue: Data Lost on Mobile
**Symptoms:** Works in Editor but not on device
**Solution:**
- Make sure `OnApplicationPause(true)` saves
- Check device logs for errors
- Verify PlayerPrefs path is writable on device

## Debug Commands

### In Unity Console or Code:
```csharp
// Check currency
GameCurrencyManager.Instance.GetDebugInfo();

// Check level progress
LevelProgressRuntimeManager.Instance.ShowProgressInfo();

// Force save
GameCurrencyManager.Instance.ForceSave();

// Reset everything
PlayerPrefs.DeleteAll();
PlayerPrefs.Save();
```

### Expected Console Output:
```
[GameSystemInitializer] Currency manager initialized
[GameSystemInitializer] Level progress manager initialized
[GameCurrencyManager] Gold loaded from PlayerPrefs: 0
[LevelProgressRuntimeManager] No saved progress, starting fresh
[LevelProgressRuntimeManager] Progress loaded: Max level 1, 0 levels

... after playing ...

Level 1 completed: 500000 points, 2 stars (Best: 500000, Stars: 2)
[LevelProgressRuntimeManager] Progress saved to PlayerPrefs: Max level 2, 1 levels
[GameCurrencyManager] Gold saved to PlayerPrefs: 450

... when clicking level ...

[PlayPopup] ConfigureLevel for level 1
[PlayPopup] ProgressManager loaded: True
[PlayPopup] LevelProgress retrieved: True
[PlayPopup] Level 1 - BestScore: 500000, Stars: 2
[PlayPopup] Score text set to: 500,000
[PlayPopup] Setting stars to: 2
```

## Success Criteria

✅ **Gold persists** across play sessions
✅ **Level progress persists** across play sessions  
✅ **Stars display correctly** on level buttons
✅ **Stars and score display** in PlayPopup
✅ **Different difficulties** have separate progress
✅ **Switching difficulties** reloads correct progress
✅ **Mobile app restart** preserves all data

## If All Tests Pass

Your save system is working correctly! The ScriptableObjects are now just bridges, and all runtime data properly saves to PlayerPrefs. 🎉
