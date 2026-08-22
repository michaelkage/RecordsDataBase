import React, { useState, useEffect } from 'react';
import { 
  StyleSheet, Text, View, TextInput, TouchableOpacity, 
  SafeAreaView, FlatList, Alert 
} from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

export default function App() {
  const [portalMode, setPortalMode] = useState('student'); // 'student' or 'admin'
  const [loginId, setLoginId] = useState('');
  const [password, setPassword] = useState('');
  const [currentUser, setCurrentUser] = useState(null);
  const [students, setStudents] = useState([]);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const storedStudents = await AsyncStorage.getItem('students_db');
      if (storedStudents) {
        setStudents(JSON.parse(storedStudents));
      } else {
        const defaultData = [{
          id: "BHS-2026-001",
          surname: "Ayeni",
          fullName: "Ayodeji Ayeni",
          department: "Science",
          classLevel: "SS3",
          classArm: "Crimson",
          subjects: ["Mathematics", "English Language", "Civic Education", "Physics", "Chemistry"],
          scores: { "Mathematics": 80, "English Language": 75, "Civic Education": 70, "Physics": 85, "Chemistry": 65 }
        }];
        await AsyncStorage.setItem('students_db', JSON.stringify(defaultData));
        setStudents(defaultData);
      }
    } catch (e) {
      console.error("Failed to load local database.", e);
    }
  };

  const calculateGrade = (score) => {
    if (score === null || score === undefined) return { grade: 'N/A', remark: 'Pending' };
    if (score >= 70) return { grade: 'A', remark: 'Excellent' };
    if (score >= 60) return { grade: 'B', remark: 'Very Good' };
    if (score >= 50) return { grade: 'C', remark: 'Good' };
    if (score >= 45) return { grade: 'D', remark: 'Pass' };
    if (score >= 40) return { grade: 'E', remark: 'Fair' };
    return { grade: 'F', remark: 'Fail' };
  };

  const handleLogin = () => {
    if (portalMode === 'admin') {
      if (password === 'admin123password') {
        setCurrentUser({ role: 'admin' });
      } else {
        Alert.alert("Access Denied", "Incorrect Admin Master Password.");
      }
    } else {
      const student = students.find(s => 
        s.id.toLowerCase() === loginId.trim().toLowerCase() && 
        s.surname.toLowerCase() === password.trim().toLowerCase()
      );
      if (student) {
        setCurrentUser({ role: 'student', data: student });
      } else {
        Alert.alert("Login Error", "Invalid Student ID or Surname password.");
      }
    }
  };

  // --- LOGIN SCREEN ---
  if (!currentUser) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.card}>
          <Text style={styles.title}>Bombi High School</Text>
          <Text style={styles.subtitle}>
            {portalMode === 'admin' ? 'Admin Control Portal' : 'Student Record Portal'}
          </Text>

          <View style={styles.tabContainer}>
            <TouchableOpacity 
              style={[styles.tab, portalMode === 'student' && styles.activeTab]} 
              onPress={() => setPortalMode('student')}>
              <Text style={styles.tabText}>Student</Text>
            </TouchableOpacity>
            <TouchableOpacity 
              style={[styles.tab, portalMode === 'admin' && styles.activeTab]} 
              onPress={() => setPortalMode('admin')}>
              <Text style={styles.tabText}>Admin</Text>
            </TouchableOpacity>
          </View>

          {portalMode === 'student' && (
            <TextInput 
              style={styles.input} 
              placeholder="Student ID (e.g. BHS-2026-001)" 
              placeholderTextColor="#94a3b8"
              value={loginId} 
              onChangeText={setLoginId} 
            />
          )}

          <TextInput 
            style={styles.input} 
            placeholder={portalMode === 'admin' ? "Master Admin Password" : "Password (Surname)"} 
            placeholderTextColor="#94a3b8"
            secureTextEntry 
            value={password} 
            onChangeText={setPassword} 
          />

          <TouchableOpacity style={styles.button} onPress={handleLogin}>
            <Text style={styles.buttonText}>
              {portalMode === 'admin' ? "Unlock Database" : "Enter Portal"}
            </Text>
          </TouchableOpacity>
        </View>
      </SafeAreaView>
    );
  }

  // --- STUDENT DASHBOARD VIEW ---
  if (currentUser.role === 'student') {
    const s = currentUser.data;
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.nameText}>{s.fullName}</Text>
          <Text style={styles.subText}>{s.id} | {s.department}</Text>
          <Text style={styles.subText}>Class: {s.classLevel} {s.classArm}</Text>
          <TouchableOpacity onPress={() => setCurrentUser(null)} style={styles.logoutBtn}>
            <Text style={styles.logoutText}>Logout</Text>
          </TouchableOpacity>
        </View>

        <Text style={styles.sectionTitle}>Academic Report Sheet</Text>
        
        <FlatList
          data={s.subjects}
          keyExtractor={(item) => item}
          renderItem={({ item }) => {
            const score = s.scores[item];
            const result = calculateGrade(score);
            return (
              <View style={styles.scoreCard}>
                <View>
                  <Text style={styles.subjectName}>{item}</Text>
                  <Text style={styles.remarkText}>{result.remark}</Text>
                </View>
                <View style={styles.scoreRight}>
                  <Text style={styles.scoreVal}>{score !== undefined ? `${score}%` : 'N/A'}</Text>
                  <Text style={styles.gradeBadge}>Grade {result.grade}</Text>
                </View>
              </View>
            );
          }}
        />
      </SafeAreaView>
    );
  }

  // --- ADMIN DIRECTORY VIEW ---
  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.nameText}>Admin Control Center</Text>
        <Text style={styles.subText}>Managing {students.length} Enrolled Students</Text>
        <TouchableOpacity onPress={() => setCurrentUser(null)} style={styles.logoutBtn}>
          <Text style={styles.logoutText}>Logout</Text>
        </TouchableOpacity>
      </View>

      <Text style={styles.sectionTitle}>Student Directory</Text>
      
      <FlatList
        data={students}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <View style={styles.studentRow}>
            <View>
              <Text style={styles.studentRowName}>{item.fullName}</Text>
              <Text style={styles.studentRowSub}>{item.id} • {item.classLevel} {item.classArm}</Text>
            </View>
            <Text style={styles.deptBadge}>{item.department}</Text>
          </View>
        )}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#0f172a', paddingHorizontal: 16, paddingTop: 40 },
  card: { backgroundColor: '#1e293b', padding: 24, borderRadius: 16, marginTop: 40 },
  title: { color: '#ffffff', fontSize: 24, fontWeight: 'bold', textAlign: 'center' },
  subtitle: { color: '#818cf8', fontSize: 13, textAlign: 'center', marginBottom: 20 },
  tabContainer: { flexDirection: 'row', backgroundColor: '#0f172a', borderRadius: 8, marginBottom: 16, padding: 4 },
  tab: { flex: 1, paddingVertical: 8, alignItems: 'center', borderRadius: 6 },
  activeTab: { backgroundColor: '#4f46e5' },
  tabText: { color: '#ffffff', fontWeight: 'bold', fontSize: 12 },
  input: { backgroundColor: '#0f172a', color: '#ffffff', padding: 12, borderRadius: 8, marginBottom: 12, borderBottomWidth: 1, borderBottomColor: '#334155' },
  button: { backgroundColor: '#4f46e5', padding: 14, borderRadius: 8, alignItems: 'center', marginTop: 8 },
  buttonText: { color: '#ffffff', fontWeight: 'bold' },
  header: { backgroundColor: '#1e293b', padding: 16, borderRadius: 12, marginBottom: 16 },
  nameText: { color: '#ffffff', fontSize: 18, fontWeight: 'bold' },
  subText: { color: '#94a3b8', fontSize: 12, marginTop: 2 },
  logoutBtn: { marginTop: 12, alignSelf: 'flex-start' },
  logoutText: { color: '#fb7185', fontWeight: 'bold', fontSize: 12 },
  sectionTitle: { color: '#ffffff', fontSize: 16, fontWeight: 'bold', marginBottom: 12 },
  scoreCard: { backgroundColor: '#1e293b', padding: 14, borderRadius: 10, flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 },
  subjectName: { color: '#ffffff', fontWeight: 'bold', fontSize: 14 },
  remarkText: { color: '#94a3b8', fontSize: 11 },
  scoreRight: { alignItems: 'flex-end' },
  scoreVal: { color: '#818cf8', fontWeight: 'bold', fontSize: 16 },
  gradeBadge: { color: '#34d399', fontSize: 11, fontWeight: 'bold' },
  studentRow: { backgroundColor: '#1e293b', padding: 14, borderRadius: 10, flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 },
  studentRowName: { color: '#ffffff', fontWeight: 'bold', fontSize: 14 },
  studentRowSub: { color: '#94a3b8', fontSize: 11 },
  deptBadge: { color: '#818cf8', fontSize: 11, fontWeight: 'bold', backgroundColor: '#0f172a', paddingHorizontal: 8, paddingVertical: 4, borderRadius: 4 }
});