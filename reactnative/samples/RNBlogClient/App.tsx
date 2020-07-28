/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 *
 * Generated with the TypeScript template
 * https://github.com/react-native-community/react-native-template-typescript
 *
 * @format
 */
import 'react-native-gesture-handler';
import * as React from 'react';
import {
  SafeAreaView,
  StyleSheet,
  ScrollView,
  View,
  Text,
  StatusBar,
  Image,
} from 'react-native';

import { NavigationContainer } from '@react-navigation/native';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { createAppContainer } from 'react-navigation';
// import Icons from 'react-native-vector-icons/FontAwesome';


function HomeScreen() {

  const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  const monthNames = ["January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
  ];
  var d = new Date();
  var dayName = days[d.getDay()];
  var date = d.getDate(); //Current Date
  var month = monthNames[d.getMonth()]; //Current Month


  return (
    <>
      <StatusBar barStyle="dark-content" />
      <SafeAreaView>
        <View style={styles.body}>
          <View
            style={{
              flexDirection: "row",
              height: 100

            }}
          >
            <View style={styles.container}>
              <View style={{ flex: 0.5 }} >
                <Text style={styles.dateTitle}>
                  {dayName} {date} {month}
                </Text>
              </View>
              <View style={{ flex: 0.5 }} >
                <Text style={styles.sectionTitle}>
                  Blog
          </Text>
              </View>
            </View>

            <View style={{ flex: 0.4 }} />
            <View style={{ flex: 0.3 }} >
              <Image source={{ uri: 'https://reactjs.org/logo-og.png' }}
                style={{ width: 50, height: 50, justifyContent: "center", marginTop: 15 }} />
            </View>

          </View>
        </View>
      </SafeAreaView>
    </>
  );
}

function EditPost() {
  return (
    <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
      <Text>Edit!</Text>
    </View>
  );
}

function Bookmarks() {
  return (
    <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
      <Text>BookMarks!</Text>
    </View>
  );
}

function Profile() {
  return (
    <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
      <Text>Profile!</Text>
    </View>
  );
}

const Tab = createBottomTabNavigator();



const App = () => {
  return (

    <NavigationContainer>
      <Tab.Navigator>
        <Tab.Screen name="Home" component={HomeScreen} />
        <Tab.Screen name="Edit" component={EditPost} />
        <Tab.Screen name="Bookmark" component={Bookmarks} />
        <Tab.Screen name="Profile" component={Profile} />
      </Tab.Navigator>
    </NavigationContainer>

  );
};

const styles = StyleSheet.create({
  body: {
    backgroundColor: '#ffffff',
    padding: 30
  },
  dateTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#ffa500',
    marginTop: 10
  },
  sectionTitle: {
    fontSize: 24,
    fontWeight: '600',
    color: '#000000',
    justifyContent: "center",
    marginTop: -10
  },
  container: {
    flex: 1,
    flexDirection: 'column'
  }
});

export default App;
