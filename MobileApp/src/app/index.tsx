// src/app/index.tsx
import React, { useEffect, useState, useMemo } from 'react';
import {
  Alert,
  FlatList,
  Modal,
  SafeAreaView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
  ActivityIndicator,
} from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

// --- Types ---
interface ScoreRecord {
  [subject: string]: number;
}

interface Student {
  id: string;
  surname: string;
  fullName: string;
  department: string;
  classLevel: string;
  classArm: string;
  subjects: string[];
  scores: ScoreRecord;
}

const STORAGE_KEY = 'students_db';

export default function App() {
  const [students, setStudents] = useState<Student[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [form, setForm] = useState<Partial<Student>>({});
  const [modalVisible, setModalVisible] = useState(false);
  const [editingStudent, setEditingStudent] = useState<Student | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setIsLoading(true);
      const stored = await AsyncStorage.getItem(STORAGE_KEY);
      if (stored) {
        setStudents(JSON.parse(stored));
      } else {
        // Initialize with default or empty
        setStudents([]); 
      }
    } catch (e) {
      Alert.alert('Error', 'Failed to load database.');
    } finally {
      setIsLoading(false);
    }
  };

  const saveStudent = async () => {
    // Basic validation
    if (!form.fullName || !form.surname) {
      Alert.alert('Error', 'Please fill in required fields.');
      return;
    }

    let nextStudents: Student[];
    if (editingStudent) {
      nextStudents = students.map((s) => 
        s.id === editingStudent.id ? { ...s, ...form } as Student : s
      );
    } else {
      const newStudent: Student = {
        id: `BHS-${Date.now()}`, // Simple unique ID
        surname: form.surname!,
        fullName: form.fullName!,
        department: form.department || 'N/A',
        classLevel: form.classLevel || 'N/A',
        classArm: form.classArm || 'N/A',
        subjects: [],
        scores: form.scores || {},
      };
      nextStudents = [newStudent, ...students];
    }

    await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(nextStudents));
    setStudents(nextStudents);
    setModalVisible(false);
  };

  if (isLoading) return <ActivityIndicator style={{ flex: 1 }} />;

  return (
    <SafeAreaView style={styles.container}>
      <Text style={styles.title}>Student Directory</Text>
      
      <FlatList
        data={students}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <View style={styles.card}>
            <Text>{item.fullName}</Text>
            {/* Added explicit UI for scores */}
            <Text>Math Score: {item.scores?.Mathematics ?? 'N/A'}</Text>
            <TouchableOpacity 
              onPress={() => {
                setEditingStudent(item);
                setForm(item);
                setModalVisible(true);
              }}
            >
              <Text style={styles.editBtn}>Edit</Text>
            </TouchableOpacity>
          </View>
        )}
      />

      <Modal visible={modalVisible}>
        <View style={styles.modalContent}>
          <TextInput 
            placeholder="Mathematics Score"
            keyboardType="numeric"
            onChangeText={(val) => setForm({ 
              ...form, 
              scores: { ...form.scores, Mathematics: Number(val) } 
            })}
          />
          <TouchableOpacity onPress={saveStudent}><Text>Save</Text></TouchableOpacity>
        </View>
      </Modal>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 20 },
  title: { fontSize: 24, fontWeight: 'bold' },
  card: { padding: 15, marginVertical: 10, backgroundColor: '#f0f0f0' },
  editBtn: { color: 'blue' },
  modalContent: { flex: 1, justifyContent: 'center', padding: 20 }
});
