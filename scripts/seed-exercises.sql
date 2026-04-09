-- Comprehensive exercise seed data
-- Covers all 5 regions with realistic exercises across all body parts and equipment types
-- Run once against ExerciseDb to populate the catalogue

SET NOCOUNT ON;

-- Clean up any soft-deleted legacy seed data
DELETE FROM Exercises WHERE IsDeleted = 1;

-- Only insert exercises that don't already exist (by name, case-insensitive)
-- Using a temp table approach for clean inserts

IF OBJECT_ID('tempdb..#SeedExercises') IS NOT NULL DROP TABLE #SeedExercises;

CREATE TABLE #SeedExercises (
    Name NVARCHAR(200),
    BodyPart NVARCHAR(100),
    TargetMuscle NVARCHAR(100),
    Equipment NVARCHAR(100) NULL,
    Description NVARCHAR(1000) NULL,
    Difficulty NVARCHAR(50) NULL
);

-- ============================================================
-- UPPER BODY — back
-- ============================================================
INSERT INTO #SeedExercises VALUES ('Barbell Bent-Over Row', 'back', 'lats', 'barbell', 'Hinge at the hips and row the bar to your lower chest.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Dumbbell Single-Arm Row', 'back', 'lats', 'dumbbell', 'Brace on a bench and row one dumbbell at a time.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Pull-Up', 'back', 'lats', 'body weight', 'Hang from a bar and pull your chin over it.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Chin-Up', 'back', 'lats', 'body weight', 'Supinated grip pull-up targeting the lats and biceps.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Lat Pulldown', 'back', 'lats', 'cable', 'Pull the bar down to your upper chest.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Seated Cable Row', 'back', 'middle back', 'cable', 'Sit upright and row the cable handle to your torso.', 'beginner');
INSERT INTO #SeedExercises VALUES ('T-Bar Row', 'back', 'middle back', 'barbell', 'Straddle a landmine bar and row with both hands.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Face Pull', 'back', 'rear deltoids', 'cable', 'Pull the rope to your face with elbows high.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Inverted Row', 'back', 'middle back', 'body weight', 'Hang beneath a bar and row your chest to meet it.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Dumbbell Pullover', 'back', 'lats', 'dumbbell', 'Lie on a bench and arc a dumbbell over your head.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Straight-Arm Pulldown', 'back', 'lats', 'cable', 'Keep arms straight and push the bar down in an arc.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Meadows Row', 'back', 'lats', 'barbell', 'Single-arm landmine row with a staggered stance.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Chest-Supported Row', 'back', 'middle back', 'dumbbell', 'Lie face-down on an incline bench and row.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Pendlay Row', 'back', 'lats', 'barbell', 'Strict bent-over row returning the bar to the floor each rep.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Machine Low Row', 'back', 'lats', 'leverage machine', 'Seated machine row with chest pad support.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Band Pull-Apart', 'back', 'rear deltoids', 'resistance band', 'Hold a band at arm''s length and pull it apart.', 'beginner');

-- ============================================================
-- UPPER BODY — chest
-- ============================================================
INSERT INTO #SeedExercises VALUES ('Barbell Bench Press', 'chest', 'pectorals', 'barbell', 'Lie flat and press the bar from your chest to lockout.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Dumbbell Bench Press', 'chest', 'pectorals', 'dumbbell', 'Press two dumbbells from chest level to full extension.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Incline Barbell Press', 'chest', 'upper pectorals', 'barbell', 'Bench press on a 30-45 degree incline.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Incline Dumbbell Press', 'chest', 'upper pectorals', 'dumbbell', 'Press dumbbells on an incline bench.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Decline Bench Press', 'chest', 'lower pectorals', 'barbell', 'Press on a decline bench to target lower chest.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Push-Up', 'chest', 'pectorals', 'body weight', 'Classic bodyweight chest exercise from the floor.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Dumbbell Fly', 'chest', 'pectorals', 'dumbbell', 'Open arms wide then squeeze dumbbells together.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Cable Crossover', 'chest', 'pectorals', 'cable', 'Step forward and bring cable handles together in an arc.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Machine Chest Press', 'chest', 'pectorals', 'leverage machine', 'Seated machine press for controlled chest work.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Pec Deck Fly', 'chest', 'pectorals', 'leverage machine', 'Seated fly machine bringing pads together.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Dip (Chest Focused)', 'chest', 'lower pectorals', 'body weight', 'Lean forward on dip bars to emphasise the chest.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Landmine Press', 'chest', 'upper pectorals', 'barbell', 'Press a landmine bar up and forward from one shoulder.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Diamond Push-Up', 'chest', 'pectorals', 'body weight', 'Hands close together forming a diamond shape.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Floor Press', 'chest', 'pectorals', 'dumbbell', 'Press dumbbells while lying on the floor for partial ROM.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Svend Press', 'chest', 'pectorals', 'weight plate', 'Squeeze two plates together and press forward.', 'beginner');

-- ============================================================
-- UPPER BODY — shoulders
-- ============================================================
INSERT INTO #SeedExercises VALUES ('Overhead Press', 'shoulders', 'anterior deltoids', 'barbell', 'Press the bar from your shoulders to overhead lockout.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Dumbbell Shoulder Press', 'shoulders', 'anterior deltoids', 'dumbbell', 'Press dumbbells overhead from shoulder height.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Arnold Press', 'shoulders', 'anterior deltoids', 'dumbbell', 'Rotate palms during the press for full delt activation.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Lateral Raise', 'shoulders', 'lateral deltoids', 'dumbbell', 'Raise dumbbells out to the sides to shoulder height.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Cable Lateral Raise', 'shoulders', 'lateral deltoids', 'cable', 'Single-arm lateral raise using a low cable.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Front Raise', 'shoulders', 'anterior deltoids', 'dumbbell', 'Raise dumbbells to the front to shoulder height.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Reverse Fly', 'shoulders', 'rear deltoids', 'dumbbell', 'Bent-over fly targeting the rear delts.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Upright Row', 'shoulders', 'lateral deltoids', 'barbell', 'Row the bar up along your body to chin height.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Machine Shoulder Press', 'shoulders', 'anterior deltoids', 'leverage machine', 'Seated overhead press on a machine.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Pike Push-Up', 'shoulders', 'anterior deltoids', 'body weight', 'Push-up with hips raised high to load the shoulders.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Barbell Shrug', 'shoulders', 'trapezius', 'barbell', 'Shrug the bar straight up by elevating the shoulders.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Dumbbell Shrug', 'shoulders', 'trapezius', 'dumbbell', 'Hold dumbbells at your sides and shrug up.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Handstand Push-Up', 'shoulders', 'anterior deltoids', 'body weight', 'Press yourself up while inverted against a wall.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Lu Raise', 'shoulders', 'lateral deltoids', 'dumbbell', 'Raise dumbbells in a wide arc with thumbs up.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Behind-the-Neck Press', 'shoulders', 'anterior deltoids', 'barbell', 'Press the bar from behind the neck overhead.', 'advanced');

-- ============================================================
-- UPPER BODY — upper arms (biceps + triceps)
-- ============================================================
INSERT INTO #SeedExercises VALUES ('Barbell Curl', 'upper arms', 'biceps', 'barbell', 'Curl a barbell from hip level to shoulder height.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Dumbbell Curl', 'upper arms', 'biceps', 'dumbbell', 'Alternating or simultaneous dumbbell curls.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Hammer Curl', 'upper arms', 'brachialis', 'dumbbell', 'Curl with a neutral grip to target the brachialis.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Preacher Curl', 'upper arms', 'biceps', 'barbell', 'Curl over a preacher bench for strict isolation.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Cable Curl', 'upper arms', 'biceps', 'cable', 'Curl using a low cable attachment.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Concentration Curl', 'upper arms', 'biceps', 'dumbbell', 'Seated curl with elbow braced on the inner thigh.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Incline Dumbbell Curl', 'upper arms', 'biceps', 'dumbbell', 'Curl from an incline bench for a deep stretch.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Spider Curl', 'upper arms', 'biceps', 'dumbbell', 'Curl draped over an incline bench face-down.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Triceps Pushdown', 'upper arms', 'triceps', 'cable', 'Push a cable bar or rope down to full arm extension.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Overhead Triceps Extension', 'upper arms', 'triceps', 'dumbbell', 'Extend a dumbbell overhead from behind the head.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Skull Crusher', 'upper arms', 'triceps', 'barbell', 'Lower the bar to your forehead then extend.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Close-Grip Bench Press', 'upper arms', 'triceps', 'barbell', 'Bench press with a narrow grip to load the triceps.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Triceps Dip', 'upper arms', 'triceps', 'body weight', 'Dip between parallel bars with an upright torso.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Triceps Kickback', 'upper arms', 'triceps', 'dumbbell', 'Extend the dumbbell behind you from a bent-over stance.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Cable Overhead Extension', 'upper arms', 'triceps', 'cable', 'Face away from a cable and extend overhead.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Bench Dip', 'upper arms', 'triceps', 'body weight', 'Dip using a bench behind you with feet on the floor.', 'beginner');

-- ============================================================
-- UPPER BODY — lower arms (forearms)
-- ============================================================
INSERT INTO #SeedExercises VALUES ('Wrist Curl', 'lower arms', 'forearm flexors', 'dumbbell', 'Curl your wrists with forearms resting on a bench.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Reverse Wrist Curl', 'lower arms', 'forearm extensors', 'dumbbell', 'Extend wrists with palms facing down.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Farmer''s Walk', 'lower arms', 'forearm flexors', 'dumbbell', 'Walk with heavy dumbbells at your sides for grip.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Dead Hang', 'lower arms', 'forearm flexors', 'body weight', 'Hang from a pull-up bar for maximum time.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Plate Pinch Hold', 'lower arms', 'forearm flexors', 'weight plate', 'Pinch two plates together and hold.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Reverse Barbell Curl', 'lower arms', 'brachioradialis', 'barbell', 'Curl with an overhand grip to target forearms.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Towel Hang', 'lower arms', 'forearm flexors', 'body weight', 'Hang from a towel draped over a pull-up bar.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Wrist Roller', 'lower arms', 'forearm flexors', 'other', 'Roll a weight up and down on a wrist roller device.', 'beginner');

-- ============================================================
-- UPPER BODY — neck
-- ============================================================
INSERT INTO #SeedExercises VALUES ('Neck Curl', 'neck', 'sternocleidomastoid', 'weight plate', 'Lie face-up and curl a plate on your forehead.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Neck Extension', 'neck', 'neck extensors', 'weight plate', 'Lie face-down and extend against a plate.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Neck Lateral Flexion', 'neck', 'sternocleidomastoid', 'body weight', 'Tilt head side to side against gentle resistance.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Band Neck Flexion', 'neck', 'sternocleidomastoid', 'resistance band', 'Use a band around the forehead for neck curls.', 'beginner');

-- ============================================================
-- LOWER BODY — upper legs
-- ============================================================
INSERT INTO #SeedExercises VALUES ('Back Squat', 'upper legs', 'quadriceps', 'barbell', 'Bar on upper back, squat to parallel and stand.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Front Squat', 'upper legs', 'quadriceps', 'barbell', 'Bar across the front delts, squat and stand.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Goblet Squat', 'upper legs', 'quadriceps', 'dumbbell', 'Hold a dumbbell at your chest and squat.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Leg Press', 'upper legs', 'quadriceps', 'leverage machine', 'Press the sled away using both legs.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Hack Squat', 'upper legs', 'quadriceps', 'leverage machine', 'Squat on a hack squat machine.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Bulgarian Split Squat', 'upper legs', 'quadriceps', 'dumbbell', 'Rear foot elevated single-leg squat.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Walking Lunge', 'upper legs', 'quadriceps', 'dumbbell', 'Step forward into alternating lunges.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Reverse Lunge', 'upper legs', 'quadriceps', 'dumbbell', 'Step backward into a lunge position.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Romanian Deadlift', 'upper legs', 'hamstrings', 'barbell', 'Hinge at the hips keeping legs nearly straight.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Conventional Deadlift', 'upper legs', 'hamstrings', 'barbell', 'Lift the bar from the floor with a hip hinge.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Sumo Deadlift', 'upper legs', 'hamstrings', 'barbell', 'Wide-stance deadlift with hands inside the knees.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Leg Curl', 'upper legs', 'hamstrings', 'leverage machine', 'Curl the pad toward your glutes.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Nordic Hamstring Curl', 'upper legs', 'hamstrings', 'body weight', 'Kneel and lower yourself forward with control.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Leg Extension', 'upper legs', 'quadriceps', 'leverage machine', 'Extend your legs against the pad from a seated position.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Hip Thrust', 'upper legs', 'glutes', 'barbell', 'Drive your hips up against a bar across your lap.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Glute Bridge', 'upper legs', 'glutes', 'body weight', 'Lie on your back and thrust hips toward the ceiling.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Step-Up', 'upper legs', 'quadriceps', 'dumbbell', 'Step onto a box or bench holding dumbbells.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Pistol Squat', 'upper legs', 'quadriceps', 'body weight', 'Single-leg squat with the other leg extended.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Wall Sit', 'upper legs', 'quadriceps', 'body weight', 'Hold a seated position against a wall.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Good Morning', 'upper legs', 'hamstrings', 'barbell', 'Bar on upper back, hinge forward at the hips.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Belt Squat', 'upper legs', 'quadriceps', 'leverage machine', 'Squat with load attached to a belt, no spinal load.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Sissy Squat', 'upper legs', 'quadriceps', 'body weight', 'Lean back and bend at the knees with heels raised.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Cable Pull-Through', 'upper legs', 'glutes', 'cable', 'Stand facing away from a cable and drive hips forward.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Dumbbell Romanian Deadlift', 'upper legs', 'hamstrings', 'dumbbell', 'Romanian deadlift performed with dumbbells.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Hip Adduction Machine', 'upper legs', 'adductors', 'leverage machine', 'Squeeze legs together on an adduction machine.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Hip Abduction Machine', 'upper legs', 'abductors', 'leverage machine', 'Push legs apart on an abduction machine.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Barbell Hip Thrust', 'upper legs', 'glutes', 'barbell', 'Back against a bench, thrust a loaded bar upward.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Single-Leg Deadlift', 'upper legs', 'hamstrings', 'dumbbell', 'Balance on one leg while hinging forward.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Trap Bar Deadlift', 'upper legs', 'quadriceps', 'trap bar', 'Deadlift inside a hexagonal bar.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Bodyweight Squat', 'upper legs', 'quadriceps', 'body weight', 'Squat using just your bodyweight.', 'beginner');

-- ============================================================
-- LOWER BODY — lower legs (calves)
-- ============================================================
INSERT INTO #SeedExercises VALUES ('Standing Calf Raise', 'lower legs', 'calves', 'leverage machine', 'Rise onto your toes under a calf raise machine.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Seated Calf Raise', 'lower legs', 'soleus', 'leverage machine', 'Rise onto toes from a seated position.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Dumbbell Calf Raise', 'lower legs', 'calves', 'dumbbell', 'Hold dumbbells and rise onto your toes.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Single-Leg Calf Raise', 'lower legs', 'calves', 'body weight', 'Rise on one foot for extra calf loading.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Donkey Calf Raise', 'lower legs', 'calves', 'leverage machine', 'Bent-over calf raise on a machine.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Leg Press Calf Raise', 'lower legs', 'calves', 'leverage machine', 'Use the leg press machine to do calf raises.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Jump Rope', 'lower legs', 'calves', 'body weight', 'Skip rope at a steady pace for calf endurance.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Tibialis Raise', 'lower legs', 'tibialis anterior', 'body weight', 'Lean against a wall and raise your toes.', 'beginner');

-- ============================================================
-- CORE — waist (abs + obliques)
-- ============================================================
INSERT INTO #SeedExercises VALUES ('Crunch', 'waist', 'abdominals', 'body weight', 'Curl your upper body off the floor.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Bicycle Crunch', 'waist', 'obliques', 'body weight', 'Twist elbow to opposite knee in a pedalling motion.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Plank', 'waist', 'abdominals', 'body weight', 'Hold a rigid position on forearms and toes.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Side Plank', 'waist', 'obliques', 'body weight', 'Hold on one forearm with body sideways.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Hanging Leg Raise', 'waist', 'abdominals', 'body weight', 'Hang from a bar and raise your legs.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Hanging Knee Raise', 'waist', 'abdominals', 'body weight', 'Hang and bring your knees to your chest.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Cable Woodchop', 'waist', 'obliques', 'cable', 'Rotate and pull a cable diagonally across your body.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Ab Rollout', 'waist', 'abdominals', 'ab wheel', 'Roll forward on an ab wheel and return.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Russian Twist', 'waist', 'obliques', 'body weight', 'Seated twist side to side with feet elevated.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Dead Bug', 'waist', 'abdominals', 'body weight', 'Lie on your back and extend opposite limbs.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Pallof Press', 'waist', 'abdominals', 'cable', 'Press a cable handle forward resisting rotation.', 'beginner');
INSERT INTO #SeedExercises VALUES ('V-Up', 'waist', 'abdominals', 'body weight', 'Simultaneously raise legs and torso to form a V.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Decline Sit-Up', 'waist', 'abdominals', 'body weight', 'Sit-up on a decline bench for extra resistance.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Mountain Climber', 'waist', 'abdominals', 'body weight', 'Drive knees toward your chest from a push-up position.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Dragon Flag', 'waist', 'abdominals', 'body weight', 'Lower and raise your entire body from a bench.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Weighted Crunch', 'waist', 'abdominals', 'weight plate', 'Crunch holding a plate on your chest.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Cable Crunch', 'waist', 'abdominals', 'cable', 'Kneel and crunch down against cable resistance.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Flutter Kick', 'waist', 'abdominals', 'body weight', 'Lie on your back and alternate small leg kicks.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Toe Touch', 'waist', 'abdominals', 'body weight', 'Reach for your toes from a lying position.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Copenhagen Plank', 'waist', 'obliques', 'body weight', 'Side plank with top leg on a bench for adductor work.', 'advanced');

-- ============================================================
-- CARDIO
-- ============================================================
INSERT INTO #SeedExercises VALUES ('Treadmill Run', 'cardio', 'cardiovascular system', 'treadmill', 'Steady-state or interval running on a treadmill.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Cycling', 'cardio', 'cardiovascular system', 'stationary bike', 'Pedal at moderate to high intensity.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Rowing Machine', 'cardio', 'cardiovascular system', 'rowing machine', 'Full-body rowing for cardio and endurance.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Elliptical', 'cardio', 'cardiovascular system', 'elliptical', 'Low-impact cardio on an elliptical trainer.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Burpee', 'cardio', 'cardiovascular system', 'body weight', 'Squat, jump back, push-up, jump up. Repeat.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Box Jump', 'cardio', 'cardiovascular system', 'body weight', 'Jump onto a raised platform and step down.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Jumping Jack', 'cardio', 'cardiovascular system', 'body weight', 'Jump spreading legs and raising arms simultaneously.', 'beginner');
INSERT INTO #SeedExercises VALUES ('High Knees', 'cardio', 'cardiovascular system', 'body weight', 'Run in place driving knees as high as possible.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Battle Rope Wave', 'cardio', 'cardiovascular system', 'battle ropes', 'Create alternating waves with heavy ropes.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Stair Climber', 'cardio', 'cardiovascular system', 'stair climber', 'Step continuously on a stair climber machine.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Kettlebell Swing', 'cardio', 'cardiovascular system', 'kettlebell', 'Swing a kettlebell between your legs and up to chest height.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Sprint Interval', 'cardio', 'cardiovascular system', 'body weight', 'Alternate short sprints with recovery periods.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Swimming', 'cardio', 'cardiovascular system', 'body weight', 'Freestyle, backstroke, or breaststroke in a pool.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Assault Bike', 'cardio', 'cardiovascular system', 'stationary bike', 'Full-body air bike with arm and leg drive.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Skater Jump', 'cardio', 'cardiovascular system', 'body weight', 'Lateral bound from foot to foot like a speed skater.', 'intermediate');

-- ============================================================
-- OTHER (uncommon or multi-region exercises)
-- ============================================================
INSERT INTO #SeedExercises VALUES ('Turkish Get-Up', 'other', 'full body', 'kettlebell', 'Stand up from lying down while holding a weight overhead.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Bear Crawl', 'other', 'full body', 'body weight', 'Crawl on hands and feet with knees hovering.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Sled Push', 'other', 'full body', 'sled', 'Push a weighted sled across the floor.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Sled Pull', 'other', 'full body', 'sled', 'Drag a sled toward you using a rope.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Sandbag Clean', 'other', 'full body', 'sandbag', 'Clean a sandbag from the floor to your shoulders.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Wall Ball', 'other', 'full body', 'medicine ball', 'Squat and throw a med ball to a target on the wall.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Thruster', 'other', 'full body', 'barbell', 'Front squat into an overhead press in one motion.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Man Maker', 'other', 'full body', 'dumbbell', 'Push-up, row, clean, press — all in one flow.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Inchworm', 'other', 'full body', 'body weight', 'Walk hands out to plank, then walk feet to hands.', 'beginner');
INSERT INTO #SeedExercises VALUES ('Clean and Jerk', 'other', 'full body', 'barbell', 'Olympic lift: clean the bar then jerk overhead.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Snatch', 'other', 'full body', 'barbell', 'Olympic lift: pull the bar from floor to overhead in one motion.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Power Clean', 'other', 'full body', 'barbell', 'Explosive pull from floor to rack position.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Muscle-Up', 'other', 'full body', 'body weight', 'Pull-up transitioned into a dip above the bar.', 'advanced');
INSERT INTO #SeedExercises VALUES ('Sledgehammer Slam', 'other', 'full body', 'other', 'Slam a sledgehammer onto a tyre.', 'intermediate');
INSERT INTO #SeedExercises VALUES ('Tyre Flip', 'other', 'full body', 'other', 'Flip a heavy tyre end over end.', 'intermediate');

-- ============================================================
-- Insert into Exercises table (skip duplicates by name)
-- ============================================================
INSERT INTO Exercises (Id, Name, BodyPart, TargetMuscle, Equipment, Description, Difficulty, GifUrl, IsDeleted, UpdatedAt, ConcurrencyToken)
SELECT
    NEWID(),
    s.Name,
    s.BodyPart,
    s.TargetMuscle,
    s.Equipment,
    s.Description,
    s.Difficulty,
    NULL,
    0,
    GETUTCDATE(),
    REPLACE(NEWID(), '-', '')
FROM #SeedExercises s
WHERE NOT EXISTS (
    SELECT 1 FROM Exercises e
    WHERE LOWER(e.Name) = LOWER(s.Name) AND e.IsDeleted = 0
);

DROP TABLE #SeedExercises;

-- Show summary
SELECT
    'Seeded' AS Status,
    COUNT(*) AS TotalExercises,
    COUNT(DISTINCT BodyPart) AS BodyParts,
    COUNT(DISTINCT Equipment) AS EquipmentTypes
FROM Exercises
WHERE IsDeleted = 0;

SELECT BodyPart, COUNT(*) AS Count
FROM Exercises
WHERE IsDeleted = 0
GROUP BY BodyPart
ORDER BY BodyPart;
